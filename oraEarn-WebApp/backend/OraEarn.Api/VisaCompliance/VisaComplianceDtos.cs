namespace OraEarn.Api.VisaCompliance;

public record VisaComplianceResponse(
    bool Applies,
    string VisaStatus,
    string WorkingRights,
    decimal FortnightHoursWorked,
    decimal FortnightHourLimit,
    decimal RemainingHours,
    bool IsOverLimit,
    bool IsNearLimit,
    DateOnly FortnightStart,
    DateOnly FortnightEnd);
