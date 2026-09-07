using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OraEarn.Api.AiAdvisor;
using OraEarn.Api.Users;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.Admin;

public class RegistrationApprovalService(
    UserManager<ApplicationUser> userManager,
    RegistrationApprovalAdvisorService advisorService,
    VisaDocumentStorageService documentStorage)
{
    public async Task<IReadOnlyList<PendingRegistrationResponse>> GetPendingAsync()
    {
        var users = await userManager.Users
            .Where(u => u.ApprovalStatus == AccountApprovalStatus.Pending)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(Map).ToList();
    }

    public async Task<PendingRegistrationResponse?> GetByIdAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is null ? null : Map(user);
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(string userId, string adminUserId, string? notes)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "Registration not found.");
        }

        if (user.ApprovalStatus == AccountApprovalStatus.Approved)
        {
            return (false, "User is already approved.");
        }

        user.ApprovalStatus = AccountApprovalStatus.Approved;
        user.ApprovedAt = DateTime.UtcNow;
        user.ApprovedByUserId = adminUserId;
        user.ApprovalNotes = notes;
        await userManager.UpdateAsync(user);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RejectAsync(string userId, string adminUserId, string? notes)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "Registration not found.");
        }

        user.ApprovalStatus = AccountApprovalStatus.Rejected;
        user.ApprovedAt = DateTime.UtcNow;
        user.ApprovedByUserId = adminUserId;
        user.ApprovalNotes = notes;
        await userManager.UpdateAsync(user);
        return (true, null);
    }

    public async Task<RegistrationAiReviewResponse?> RunAiReviewAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var registration = Map(user);
        var review = advisorService.Review(registration, !string.IsNullOrWhiteSpace(user.VisaDocumentPath));
        user.AiReviewSummary = $"{review.Recommendation}: {review.Summary}";
        await userManager.UpdateAsync(user);
        return review;
    }

    public (string? FullPath, string? FileName, string? ContentType) GetDocumentInfo(ApplicationUser user)
    {
        var fullPath = documentStorage.GetFullPath(user.VisaDocumentPath);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return (null, null, null);
        }

        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

        return (fullPath, user.VisaDocumentFileName ?? Path.GetFileName(fullPath), contentType);
    }

    private static PendingRegistrationResponse Map(ApplicationUser user) =>
        new(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.PhoneNumber ?? string.Empty,
            user.DateOfBirth,
            user.Nationality,
            user.VisaType,
            user.VisaStatus,
            user.WorkingRights,
            user.VisaExpiryDate,
            user.IdentityDocumentType,
            user.VisaDocumentFileName,
            !string.IsNullOrWhiteSpace(user.VisaDocumentPath),
            user.ApprovalStatus,
            user.AiReviewSummary,
            user.CreatedAt);
}
