using OraEarn.Api.Users;

namespace OraEarn.Api.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateOnly DateOfBirth,
    string Nationality,
    string VisaType,
    string VisaStatus,
    string WorkingRights,
    DateOnly VisaExpiryDate,
    IdentityDocumentType IdentityDocumentType);

public record RegisterResponse(
    string Message,
    string UserId,
    AccountApprovalStatus ApprovalStatus);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserProfileResponse User);

public record UserProfileResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    AccountApprovalStatus ApprovalStatus);
