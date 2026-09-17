using Microsoft.AspNetCore.Identity;
using OraEarn.Api.Expenses;
using OraEarn.Api.Jobs;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Shifts;

namespace OraEarn.Api.Users;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public string VisaType { get; set; } = string.Empty;
    public string VisaStatus { get; set; } = string.Empty;
    public string WorkingRights { get; set; } = string.Empty;
    public DateOnly? VisaExpiryDate { get; set; }
    public IdentityDocumentType IdentityDocumentType { get; set; }
    public string? VisaDocumentPath { get; set; }
    public string? VisaDocumentFileName { get; set; }
    public AccountApprovalStatus ApprovalStatus { get; set; } = AccountApprovalStatus.Pending;
    public string? ApprovalNotes { get; set; }
    public string? AiReviewSummary { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserId { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Job> Jobs { get; set; } = [];
    public ICollection<ActiveShiftSession> ActiveShiftSessions { get; set; } = [];
    public ICollection<ExpenseCategory> ExpenseCategories { get; set; } = [];
    public ICollection<SavingsGoal> SavingsGoals { get; set; } = [];
}
