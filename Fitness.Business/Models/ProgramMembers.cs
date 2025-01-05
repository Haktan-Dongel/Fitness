namespace Fitness.Business.Models
{
    public class ProgramMembers
    {
        public string ProgramCode { get; set; } = string.Empty;
        public int MemberId { get; set; }
        public FitnessProgram Program { get; set; } = null!;
        public Member Member { get; set; } = null!;
    }
}
