using OraEarn.Api.Admin;
using OraEarn.Api.Users;

namespace OraEarn.Api.AiAdvisor;

// The admin "AI review" (FR-1.6) is a deterministic rule checklist that runs inside the API; it does
// not call the Aura service, so it works the same whether Aura is running or not. It only advises:
// approval is always an explicit admin action in RegistrationApprovalService.
public class RegistrationApprovalAdvisorService
{
    /// <summary>
    /// Scores a pending registration against completeness and visa-validity checks and returns a
    /// recommendation with the passed checks and raised flags for the admin to review.
    /// </summary>
    public RegistrationAiReviewResponse Review(PendingRegistrationResponse registration, bool documentExists)
    {
        // Scoring: one point per complete profile field, two each for an unexpired visa and an
        // uploaded document, because those are what the admin must actually verify. The weights and
        // the approve threshold below are the team's heuristics, not a published standard.
        var checks = new List<string>();
        var flags = new List<string>();
        var score = 0;

        if (!string.IsNullOrWhiteSpace(registration.FirstName) && !string.IsNullOrWhiteSpace(registration.LastName))
        {
            checks.Add("Full name provided.");
            score += 1;
        }
        else
        {
            flags.Add("Name fields are incomplete.");
        }

        if (!string.IsNullOrWhiteSpace(registration.Email) && registration.Email.Contains('@'))
        {
            checks.Add("Valid email format.");
            score += 1;
        }
        else
        {
            flags.Add("Email looks invalid.");
        }

        if (!string.IsNullOrWhiteSpace(registration.PhoneNumber) && registration.PhoneNumber.Length >= 8)
        {
            checks.Add("Phone number provided.");
            score += 1;
        }
        else
        {
            flags.Add("Phone number missing or too short.");
        }

        if (registration.DateOfBirth.HasValue)
        {
            // Year difference only, so this can be one year high before the birthday; good enough
            // for a plausibility check, which is why the message says "approx.".
            var age = DateOnly.FromDateTime(DateTime.UtcNow).Year - registration.DateOfBirth.Value.Year;
            if (age >= 16 && age <= 80)
            {
                checks.Add($"Date of birth plausible (approx. age {age}).");
                score += 1;
            }
            else
            {
                flags.Add($"Date of birth unusual (approx. age {age}).");
            }
        }
        else
        {
            flags.Add("Date of birth missing.");
        }

        if (!string.IsNullOrWhiteSpace(registration.Nationality))
        {
            checks.Add($"Nationality: {registration.Nationality}.");
            score += 1;
        }
        else
        {
            flags.Add("Nationality missing.");
        }

        if (!string.IsNullOrWhiteSpace(registration.VisaType))
        {
            checks.Add($"Visa type: {registration.VisaType}.");
            score += 1;
        }
        else
        {
            flags.Add("Visa type missing.");
        }

        if (!string.IsNullOrWhiteSpace(registration.VisaStatus))
        {
            checks.Add($"Visa status: {registration.VisaStatus}.");
            score += 1;
        }
        else
        {
            flags.Add("Visa status not declared.");
        }

        if (!string.IsNullOrWhiteSpace(registration.WorkingRights))
        {
            checks.Add($"Working rights: {registration.WorkingRights}.");
            score += 1;

            if (registration.VisaStatus.Equals("Student", StringComparison.OrdinalIgnoreCase) &&
                !registration.WorkingRights.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                flags.Add("Student visa but working rights are not Student — verify hours limit.");
            }

            if (registration.VisaStatus.Equals("Citizen", StringComparison.OrdinalIgnoreCase) &&
                registration.WorkingRights.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                flags.Add("Citizen with Student working rights looks inconsistent.");
            }
        }
        else
        {
            flags.Add("Working rights not declared.");
        }

        if (registration.VisaExpiryDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (registration.VisaExpiryDate.Value >= today)
            {
                var daysLeft = registration.VisaExpiryDate.Value.DayNumber - today.DayNumber;
                checks.Add($"Visa valid until {registration.VisaExpiryDate.Value:d} ({daysLeft} days left).");
                score += 2;
                if (daysLeft < 30)
                {
                    flags.Add("Visa expires within 30 days — verify renewal.");
                }
            }
            else
            {
                flags.Add($"Visa expired on {registration.VisaExpiryDate.Value:d}.");
            }
        }
        else
        {
            flags.Add("Visa expiry date missing.");
        }

        if (documentExists)
        {
            checks.Add($"Identity proof uploaded: {FormatDocumentType(registration.IdentityDocumentType)}.");
            score += 2;
        }
        else
        {
            flags.Add("No identity document on file.");
        }

        // Suggest approval only with a document, no expired visa and at most one minor flag.
        // Note: the "No visa document" test in the reject branch never matches, because the flag
        // text above is "No identity document on file."; a missing document therefore leads to
        // "Manual review recommended" rather than "Reject or request resubmission".
        var suggestApprove = score >= 7 && documentExists &&
            !flags.Any(f => f.Contains("expired", StringComparison.OrdinalIgnoreCase)) &&
            flags.Count <= 1;
        var recommendation = suggestApprove ? "Approve" : flags.Any(f => f.Contains("expired") || f.Contains("No visa document")) ? "Reject or request resubmission" : "Manual review recommended";

        var summary = suggestApprove
            ? "AI review: registration looks complete with a valid visa timeline and document. Admin may approve."
            : flags.Count > 0
                ? $"AI review: found {flags.Count} issue(s). Admin should verify before approving."
                : "AI review: basic checks passed. Admin should still confirm the visa document manually.";

        return new RegistrationAiReviewResponse(recommendation, summary, checks, flags, suggestApprove);
    }

    private static string FormatDocumentType(IdentityDocumentType type) => type switch
    {
        IdentityDocumentType.Passport => "Passport",
        IdentityDocumentType.DrivingLicence => "Driving licence",
        IdentityDocumentType.NationalIdCard => "National ID card",
        IdentityDocumentType.VisaGrantNotice => "Visa grant notice",
        _ => type.ToString()
    };
}
