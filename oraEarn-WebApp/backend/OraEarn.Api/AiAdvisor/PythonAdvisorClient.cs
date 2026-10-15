using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OraEarn.Api.SavingsGoals;

namespace OraEarn.Api.AiAdvisor;

// Typed HttpClient for the FastAPI Aura service (AiAdvisor:BaseUrl, default http://localhost:8000).
public class PythonAdvisorClient(HttpClient http, IOptions<AiAdvisorSettings> settings, ILogger<PythonAdvisorClient> logger)
{
    // snake_case matches Python/Pydantic field naming, so neither side needs aliases. Nulls are
    // omitted so optional Pydantic fields fall back to their defaults.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Posts a chat request to Aura. Returns null - never throws - when Aura is unreachable, returns
    /// an error status or sends an unreadable body, so the caller can choose the fallback.
    /// </summary>
    public async Task<AdvisorChatResponse?> ChatAsync(PythonAdvisorRequest request, CancellationToken ct = default)
    {
        var baseUrl = settings.Value.BaseUrl.TrimEnd('/');
        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/advisor/chat")
            {
                Content = JsonContent.Create(request, options: JsonOptions),
            };
            // Optional shared secret so a deployed Aura only answers this API. Local development
            // usually leaves it empty.
            if (!string.IsNullOrWhiteSpace(settings.Value.ApiKey))
            {
                message.Headers.Add("X-Aura-Key", settings.Value.ApiKey);
            }

            using var response = await http.SendAsync(message, ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Python advisor returned {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AdvisorChatResponse>(JsonOptions, ct);
        }
        // Deliberately broad: connection refused, timeouts and bad JSON all mean "Aura unavailable"
        // to the user. Logged as a warning, not an error, because running without Aura is supported.
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Python advisor unavailable at {BaseUrl}", baseUrl);
            return null;
        }
    }
}