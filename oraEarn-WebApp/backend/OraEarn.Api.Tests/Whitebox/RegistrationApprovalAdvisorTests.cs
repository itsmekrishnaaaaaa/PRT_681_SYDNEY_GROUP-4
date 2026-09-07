using OraEarn.Api.Admin;
using OraEarn.Api.AiAdvisor;
using OraEarn.Api.Users;

namespace OraEarn.Api.Tests.Whitebox;

public class RegistrationApprovalAdvisorTests
{
    private readonly RegistrationApprovalAdvisorService _advisor = new();

    [Fact]
    public void Review_CompleteRegistration_SuggestsApprove()
    {
        var registration = ValidRegistration();

        var result = _advisor.Review(registration, documentExists: true);

        Assert.True(result.SuggestApprove);
        Assert.Empty(result.Flags);
        Assert.NotEmpty(result.Checks);
    }

    [Fact]
    public void Review_MissingDocument_AddsFlag()
    {
        var registration = ValidRegistration();

        var result = _advisor.Review(registration, documentExists: false);

        Assert.False(result.SuggestApprove);
        Assert.Contains(result.Flags, f => f.Contains("document", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Review_ExpiredVisa_AddsFlag()
    {
        var registration = ValidRegistration() with { VisaExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) };

        var result = _advisor.Review(registration, documentExists: true);

        Assert.False(result.SuggestApprove);
        Assert.Contains(result.Flags, f => f.Contains("expired", StringComparison.OrdinalIgnoreCase));
    }

    private static PendingRegistrationResponse ValidRegistration() => new(
        "user-1",
        "alice@example.com",
        "Alice",
        "Smith",
        "+61412345678",
        DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-22)),
        "Australian",
        "Subclass 500 – Student",
        "Student",
        "Student",
        DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
        IdentityDocumentType.Passport,
        "passport.pdf",
        HasVisaDocument: true,
        AccountApprovalStatus.Pending,
        AiReviewSummary: null,
        CreatedAt: DateTime.UtcNow);
}
