namespace OraEarn.Api.Shifts;

public record ShiftRequest(
    Guid JobId,
    DateOnly WorkDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int BreakMinutes);

public record ShiftResponse(
    Guid Id,
    Guid JobId,
    string JobTitle,
    DateOnly WorkDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int BreakMinutes,
    decimal TotalHours,
    decimal TotalEarned);

public record ShiftSummaryResponse(
    decimal TotalHours,
    decimal TotalPay,
    int ShiftCount);

public record ClockInRequest(Guid JobId);

public record ActiveShiftSessionResponse(
    Guid Id,
    Guid JobId,
    string JobTitle,
    DateOnly ShiftDate,
    TimeOnly StartTime,
    int BreakMinutesAccumulated,
    DateTime? BreakStartedAt,
    decimal HourlyRate);
