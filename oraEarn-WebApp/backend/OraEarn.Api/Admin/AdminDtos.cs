using OraEarn.Api.Users;

namespace OraEarn.Api.Admin;

public record AdminUserResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt,
    AccountApprovalStatus ApprovalStatus);

public record UpdateUserRolesRequest(IReadOnlyList<string> Roles);

public record PendingRegistrationResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateOnly? DateOfBirth,
    string Nationality,
    string VisaType,
    string VisaStatus,
    string WorkingRights,
    DateOnly? VisaExpiryDate,
    IdentityDocumentType IdentityDocumentType,
    string? VisaDocumentFileName,
    bool HasVisaDocument,
    AccountApprovalStatus ApprovalStatus,
    string? AiReviewSummary,
    DateTime CreatedAt);

public record RegistrationDecisionRequest(string? Notes);

public record RegistrationAiReviewResponse(
    string Recommendation,
    string Summary,
    IReadOnlyList<string> Checks,
    IReadOnlyList<string> Flags,
    bool SuggestApprove);
