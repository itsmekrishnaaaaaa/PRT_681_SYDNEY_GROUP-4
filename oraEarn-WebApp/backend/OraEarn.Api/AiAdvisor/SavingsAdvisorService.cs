using Microsoft.AspNetCore.Identity;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Users;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.AiAdvisor;

// Backend half of the Aura chat (FR-9.x). The .NET API owns the data and builds a snapshot of it;
// the Python service only turns that snapshot plus the user's message into advice. Aura never
// queries the database, so it cannot see other users' data or anything not put in this context.
public class SavingsAdvisorService(
    SavingsGoalAnalyticsService analyticsService,
    VisaComplianceService visaComplianceService,
    PythonAdvisorClient pythonAdvisor,
    UserManager<ApplicationUser> userManager)
{
    /// <summary>
    /// Sends the user's message to Aura with a snapshot of their profile, visa hours and goals.
    /// Returns a fixed "offline" reply instead of failing when the Python service is unreachable.
    /// </summary>
    public async Task<AdvisorChatResponse> ChatAsync(string userId, string message)
    {
        var user = await userManager.FindByIdAsync(userId);
        var analytics = await analyticsService.GetAnalyticsAsync(userId);
        var visa = await visaComplianceService.GetStatusAsync(userId);

        // Context shape (serialised as snake_case JSON by PythonAdvisorClient):
        //   user      - name, email, nationality and visa fields from the profile
        //   visa      - the same fortnight-hours status the dashboard shows
        //   analytics - net balance, allocation %, monthly income estimate, per-goal progress and
        //               hours needed, plus the notifications currently raised for the user
        // Recomputed on every message so Aura always answers from current numbers. Any field
        // added here must also be accepted by the Pydantic models in ai-advisor/.
        var context = new PythonAdvisorContext(
            new PythonUserContext(
                user?.FirstName ?? "",
                user?.LastName ?? "",
                user?.Email ?? "",
                user?.Nationality ?? "",
                user?.VisaType ?? "",
                user?.VisaStatus ?? "",
                user?.WorkingRights ?? "",
                user?.VisaExpiryDate?.ToString("yyyy-MM-dd")),
            new PythonVisaContext(
                visa.Applies,
                visa.VisaStatus,
                visa.WorkingRights,
                visa.FortnightHoursWorked,
                visa.FortnightHourLimit,
                visa.RemainingHours,
                visa.IsOverLimit,
                visa.IsNearLimit),
            new PythonAnalyticsContext(
                analytics.NetBalance,
                analytics.TotalAllocationPercent,
                analytics.MonthlyIncomeEstimate,
                analytics.Goals.Select(g => new PythonGoalContext(
                    g.Title,
                    g.Status.ToString(),
                    g.HoursNeeded,
                    g.AvgHourlyRate,
                    g.AvgHoursPerWeek,
                    g.RemainingAmount,
                    g.EstimatedFinishDate?.ToString("yyyy-MM-dd"),
                    g.WeeksToComplete)).ToList(),
                analytics.Notifications.Select(n => new PythonNotificationContext(
                    n.Type,
                    n.Message,
                    n.Severity)).ToList()));

        var pythonResponse = await pythonAdvisor.ChatAsync(new PythonAdvisorRequest(message, context));
        if (pythonResponse is not null)
        {
            return pythonResponse;
        }

        // Aura is optional infrastructure: when it is down the chat degrades to a clear message
        // with HTTP 200, so the widget keeps working and the rest of the app is unaffected.
        // There is no local rule-based savings advice in this fallback.
        return new AdvisorChatResponse(
            "Aura is offline right now — start the Python service and try again.",
            []);
    }
}
