using Microsoft.AspNetCore.Identity;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Users;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.AiAdvisor;

public class SavingsAdvisorService(
    SavingsGoalAnalyticsService analyticsService,
    VisaComplianceService visaComplianceService,
    PythonAdvisorClient pythonAdvisor,
    UserManager<ApplicationUser> userManager)
{
    public async Task<AdvisorChatResponse> ChatAsync(string userId, string message)
    {
        var user = await userManager.FindByIdAsync(userId);
        var analytics = await analyticsService.GetAnalyticsAsync(userId);
        var visa = await visaComplianceService.GetStatusAsync(userId);

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

        return new AdvisorChatResponse(
            "Aura is offline right now — start the Python service and try again.",
            []);
    }
}
