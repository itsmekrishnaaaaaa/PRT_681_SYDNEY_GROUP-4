namespace OraEarn.Api.AiAdvisor;

public class AiAdvisorSettings
{
    public const string SectionName = "AiAdvisor";
    public string BaseUrl { get; set; } = "http://localhost:8000";
    public string? ApiKey { get; set; }
}
