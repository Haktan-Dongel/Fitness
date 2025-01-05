namespace Fitness.Business.DTOs
{
    public record TrainingSessionDto(
        int SessionId,
        DateTime Date,
        int Duration,
        string Type,
        string Details
    );
}
