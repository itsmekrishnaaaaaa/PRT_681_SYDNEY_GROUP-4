using Microsoft.AspNetCore.Identity;
using OraEarn.Api.Constants;
using OraEarn.Api.Users;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.Auth;

// Registration and sign-in. Expected failures (validation, duplicate email, pending approval, bad
// password) are returned as an error message rather than thrown, so the controller can map them to
// 400/401 without exception handling.
public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    TokenService tokenService,
    VisaDocumentStorageService documentStorage)
{
    /// <summary>
    /// Creates a Pending user with the uploaded identity document. The user cannot sign in until an
    /// admin approves the registration (NFR-7.3).
    /// </summary>
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

            // There is no email verification step; admin approval is the gate instead.
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return (false, null, string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        // The file name needs the new user's id, so the user is created first and deleted again if
        // the document cannot be stored. This rollback is the only place a user is ever deleted.
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

    /// <summary>
    /// Signs in an approved user (or any admin) and returns a new token pair. Failed passwords count
    /// towards the Identity lockout.
    /// </summary>
    public async Task<(bool Success, AuthResponse? Response, string? Error)> LoginAsync(LoginRequest request)
    {
        // Unknown email and wrong password give the same message so the login form cannot be used
        // to discover which emails are registered.
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return (false, null, "Invalid email or password.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains(Roles.Admin);

        // Admins skip the approval gate (the seeder creates them already approved). Note the order:
        // the approval check runs before the password check, so the pending/rejected message is
        // shown even without the correct password and reveals that the email is registered. Whether
        // that was intended is not recorded; moving the check after the password would close it.
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

    /// <summary>Returns the signed-in user's profile and roles, or null if the user no longer exists.</summary>
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

        // Early, friendlier length message; the full policy (upper, lower, digit) is enforced by
        // Identity in CreateAsync.
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

        // Keeps the two fields consistent so VisaComplianceService reliably applies the 48-hour rule.
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

    // Accepts the common ways Australians write a number (0412 345 678, +61 412 345 678, 61412345678)
    // by normalising to +61 and then requiring a landline (2, 3, 7, 8) or mobile (4) prefix and
    // eight more digits.
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
