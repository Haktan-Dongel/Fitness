namespace Fitness.Business.DTOs
{
    public record MonthlySessionsDto(

    int Year,

    int Month,

    int TotalSessions,

    double TotalHours,

    int AverageDuration,

    Dictionary<string, int> SessionsByType,

    IEnumerable<SessionSummaryDto> Sessions

);


    public record SessionSummaryDto(
        DateTime Date,
        string Type,
        int DurationMinutes,
        string Details,
        string TrainingImpact
    );

    public record TrainingStatisticsDto(
        int TotalSessions,
        double TotalHours,
        int ShortestSessionMinutes,
        int LongestSessionMinutes,
        double AverageSessionMinutes,
        Dictionary<string, int> SessionsByType
    );
}
