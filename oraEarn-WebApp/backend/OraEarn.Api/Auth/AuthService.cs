using Microsoft.AspNetCore.Identity;
using OraEarn.Api.Constants;
using OraEarn.Api.Users;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.Auth;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    TokenService tokenService,
    VisaDocumentStorageService documentStorage)
{
    public async Task<(bool Success, RegisterResponse? Response, string? Error)> RegisterAsync(
        RegisterRequest request,
        IFormFile? visaDocument)
    {
        var validationError = ValidateRegistration(request, visaDocument);
        if (validationError is not null)
        {
            return (false, null, validationError);
        }

        var existing = await userManager.FindByEmailAsync(request.Email.Trim());
        if (existing is not null)
        {
            return (false, null, "Email is already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            DateOfBirth = request.DateOfBirth,
            Nationality = request.Nationality.Trim(),
            VisaType = request.VisaType.Trim(),
            VisaStatus = request.VisaStatus.Trim(),
            WorkingRights = request.WorkingRights.Trim(),
            VisaExpiryDate = request.VisaExpiryDate,
            IdentityDocumentType = request.IdentityDocumentType,
            ApprovalStatus = AccountApprovalStatus.Pending,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return (false, null, string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        var (saved, path, saveError) = await documentStorage.SaveAsync(visaDocument!, user.Id);
        if (!saved)
        {
            await userManager.DeleteAsync(user);
            return (false, null, saveError);
        }

        user.VisaDocumentPath = path;
        user.VisaDocumentFileName = visaDocument!.FileName;
        await userManager.UpdateAsync(user);

        await userManager.AddToRoleAsync(user, Roles.User);

        return (true, new RegisterResponse(
            "Registration submitted. An admin must approve your account before you can sign in.",
            user.Id,
            AccountApprovalStatus.Pending), null);
    }

    public async Task<(bool Success, AuthResponse? Response, string? Error)> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return (false, null, "Invalid email or password.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains(Roles.Admin);

        if (!isAdmin)
        {
            if (user.ApprovalStatus == AccountApprovalStatus.Pending)
            {
                return (false, null, "Your registration is pending admin approval.");
            }

            if (user.ApprovalStatus == AccountApprovalStatus.Rejected)
            {
                return (false, null, "Your registration was rejected. Contact support or re-register with corrected details.");
            }
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return (false, null, result.IsLockedOut
                ? "Account is locked. Try again later."
                : "Invalid email or password.");
        }

        var auth = await tokenService.CreateAuthResponseAsync(user);
        return (true, auth, null);
    }

    public async Task<UserProfileResponse?> GetProfileAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return TokenService.MapUser(user, roles);
    }

    private static string? ValidateRegistration(RegisterRequest request, IFormFile? visaDocument)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return "First and last name are required.";
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return "A valid email is required.";
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber) || !IsValidAustralianPhone(request.PhoneNumber.Trim()))
        {
            return "A valid Australian phone number is required (e.g. +61412345678).";
        }

        if (request.Password != request.ConfirmPassword)
        {
            return "Password and confirmation do not match.";
        }

        if (request.Password.Length < 8)
        {
            return "Password must be at least 8 characters.";
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (request.DateOfBirth > today.AddYears(-15))
        {
            return "You must be at least 15 years old to register.";
        }

        if (string.IsNullOrWhiteSpace(request.Nationality))
        {
            return "Nationality is required.";
        }

        if (string.IsNullOrWhiteSpace(request.VisaType))
        {
            return "Visa type is required.";
        }

        if (string.IsNullOrWhiteSpace(request.VisaStatus))
        {
            return "Visa status is required.";
        }

        if (string.IsNullOrWhiteSpace(request.WorkingRights))
        {
            return "Working rights is required.";
        }

        if (request.VisaStatus.Equals("Student", StringComparison.OrdinalIgnoreCase) &&
            !request.WorkingRights.Equals("Student", StringComparison.OrdinalIgnoreCase))
        {
            return "Student visa status requires Student working rights (limited hours).";
        }

        if (!request.VisaStatus.Equals("Citizen", StringComparison.OrdinalIgnoreCase) &&
            request.VisaExpiryDate < today)
        {
            return "Visa expiry date must be in the future.";
        }

        if (!Enum.IsDefined(request.IdentityDocumentType))
        {
            return "Select a valid identity document type (passport, driving licence, or ID card).";
        }

        if (visaDocument is null || visaDocument.Length == 0)
        {
            return "Upload your identity document (PDF or image).";
        }

        return null;
    }

    private static bool IsValidAustralianPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("61"))
        {
            digits = digits[2..];
        }
        else if (digits.StartsWith('0'))
        {
            digits = digits[1..];
        }

        while (digits.StartsWith('0'))
        {
            digits = digits[1..];
        }

        return System.Text.RegularExpressions.Regex.IsMatch("+61" + digits, @"^\+61[23478]\d{8}$");
    }
}
