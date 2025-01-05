using Fitness.Business.Models;

namespace Fitness.Business.DTOs
{
    public class CreateRunningSessionDto
    {
        public DateTime Date { get; set; }
        public int MemberId { get; set; }
        public int Duration { get; set; }
        public double AvgSpeed { get; set; }
        public List<RunningIntervalDto> Intervals { get; set; } = new();
    }

    public class RunningIntervalDto
    {
        public int SeqNr { get; set; }
        public int IntervalTime { get; set; }
        public double IntervalSpeed { get; set; }
    }
}
