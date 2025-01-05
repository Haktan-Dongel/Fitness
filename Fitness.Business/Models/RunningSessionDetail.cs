namespace Fitness.Business.Models
{
    public class RunningSessionDetail
    {
        public int RunningSessionId { get; set; }
        public int SeqNr { get; set; }
        public int IntervalTime { get; set; }
        public double IntervalSpeed { get; set; }
        public RunningSessionMain RunningSession { get; set; } = null!;
    }
}
