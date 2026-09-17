namespace OraEarn.Api.Jobs;

public record JobRequest(string Title, decimal HourlyRate, DateOnly DateOfJoin);

public record JobResponse(
    Guid Id,
    string Title,
    decimal HourlyRate,
    DateOnly DateOfJoin);
