using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OraEarn.Api.SavingsGoals;

namespace OraEarn.Api.AiAdvisor;

public class PythonAdvisorClient(HttpClient http, IOptions<AiAdvisorSettings> settings, ILogger<PythonAdvisorClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<AdvisorChatResponse?> ChatAsync(PythonAdvisorRequest request, CancellationToken ct = default)
    {
        var baseUrl = settings.Value.BaseUrl.TrimEnd('/');
        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/advisor/chat")
            {
                Content = JsonContent.Create(request, options: JsonOptions),
            };
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
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Python advisor unavailable at {BaseUrl}", baseUrl);
            return null;
        }
    }
}