namespace OraEarn.Api.Data;

public class SeedOptions
{
    public const string SectionName = "Seed";

    public bool DemoData { get; set; }
    public string? AdminEmail { get; set; }
    public string? AdminPassword { get; set; }
}
