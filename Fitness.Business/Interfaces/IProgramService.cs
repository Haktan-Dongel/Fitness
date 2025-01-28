namespace Fitness.Business.Interfaces
{
    public interface IProgramService
    {
        Task<bool> HasAvailableSpaceAsync(string programCode);
        Task AddMemberToProgramAsync(string programCode, int memberId);
    }
}
