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

/// <summary>
/// Single multipart form so Swagger can document register + ID upload.
/// Field names match the existing frontend FormData.
/// </summary>
public class RegisterForm
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public string VisaType { get; set; } = string.Empty;
    public string VisaStatus { get; set; } = string.Empty;
    public string WorkingRights { get; set; } = string.Empty;
    public DateOnly VisaExpiryDate { get; set; }
    public IdentityDocumentType IdentityDocumentType { get; set; }
    public IFormFile? VisaDocument { get; set; }

    public RegisterRequest ToRequest() => new(
        Email,
        Password,
        ConfirmPassword,
        FirstName,
        LastName,
        PhoneNumber,
        DateOfBirth,
        Nationality,
        VisaType,
        VisaStatus,
        WorkingRights,
        VisaExpiryDate,
        IdentityDocumentType);
}

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
