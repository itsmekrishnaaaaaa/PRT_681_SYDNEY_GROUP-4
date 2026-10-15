using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OraEarn.Api.AiAdvisor;
using OraEarn.Api.Users;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.Admin;

// New accounts start as Pending and cannot sign in until an admin approves them (AuthService checks the
// status at login). This service holds the admin side of that gate.
public class RegistrationApprovalService(
    UserManager<ApplicationUser> userManager,
    RegistrationApprovalAdvisorService advisorService,
    VisaDocumentStorageService documentStorage)
{
    /// <summary>Lists registrations still awaiting a decision, oldest first.</summary>
    public async Task<IReadOnlyList<PendingRegistrationResponse>> GetPendingAsync()
    {
        var users = await userManager.Users
            .Where(u => u.ApprovalStatus == AccountApprovalStatus.Pending)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(Map).ToList();
    }

    /// <summary>Returns one registration's details whatever its status, or null if the user does not exist.</summary>
    public async Task<PendingRegistrationResponse?> GetByIdAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is null ? null : Map(user);
    }

    /// <summary>Approves a registration and records who decided, when, and any notes.</summary>
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

    /// <summary>Rejects a registration and records who decided, when, and any notes.</summary>
    public async Task<(bool Success, string? Error)> RejectAsync(string userId, string adminUserId, string? notes)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "Registration not found.");
        }

        // Unlike ApproveAsync there is no status guard, so an approved user can be rejected later. That blocks new
        // sign-ins, but an existing refresh token keeps working because refresh does not re-check approval.
        // ApprovedAt/ApprovedByUserId hold the decision time and admin for rejections too.
        user.ApprovalStatus = AccountApprovalStatus.Rejected;
        user.ApprovedAt = DateTime.UtcNow;
        user.ApprovedByUserId = adminUserId;
        user.ApprovalNotes = notes;
        await userManager.UpdateAsync(user);
        return (true, null);
    }

    /// <summary>
    /// Runs the local rule checklist (no call to the Python service) and saves a one-line result on the user
    /// so it shows in the pending list. The admin still makes the decision.
    /// </summary>
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

    /// <summary>Resolves the stored visa document for the admin download endpoint, or nulls if the file is missing.</summary>
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
            // Uploads are limited to pdf/jpg/jpeg/png/webp, so the fallback only covers .jpg and .jpeg.
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
