namespace OraEarn.Api.AiAdvisor;

public record AdvisorChatRequest(string Message);

public record AdvisorChatResponse(
    string Reply,
    IReadOnlyList<string> Suggestions);

public record PythonAdvisorRequest(string Message, PythonAdvisorContext Context);

public record PythonAdvisorContext(
    PythonUserContext User,
    PythonVisaContext Visa,
    PythonAnalyticsContext Analytics);

public record PythonUserContext(
    string FirstName,
    string LastName,
    string Email,
    string Nationality,
    string VisaType,
    string VisaStatus,
    string WorkingRights,
    string? VisaExpiryDate);

public record PythonVisaContext(
    bool Applies,
    string VisaStatus,
    string WorkingRights,
    decimal FortnightHoursWorked,
    decimal FortnightHourLimit,
    decimal RemainingHours,
    bool IsOverLimit,
    bool IsNearLimit);

public record PythonAnalyticsContext(
    decimal NetBalance,
    decimal TotalAllocationPercent,
    decimal MonthlyIncomeEstimate,
    IReadOnlyList<PythonGoalContext> Goals,
    IReadOnlyList<PythonNotificationContext> Notifications);

public record PythonGoalContext(
    string Title,
    string Status,
    decimal HoursNeeded,
    decimal AvgHourlyRate,
    decimal AvgHoursPerWeek,
    decimal RemainingAmount,
    string? EstimatedFinishDate,
    int? WeeksToComplete);

public record PythonNotificationContext(
    string Type,
    string Message,
    string Severity);
