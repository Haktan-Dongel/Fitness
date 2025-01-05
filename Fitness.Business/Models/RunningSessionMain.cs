namespace Fitness.Business.Models
{
    public class RunningSessionMain
    {
        public int RunningSessionId { get; set; }
        public DateTime Date { get; set; }
        public int MemberId { get; set; }
        public int Duration { get; set; }
        public double AvgSpeed { get; set; }
        public Member Member { get; set; } = null!;
        public ICollection<RunningSessionDetail> Details { get; set; } = new List<RunningSessionDetail>();
    }
}
