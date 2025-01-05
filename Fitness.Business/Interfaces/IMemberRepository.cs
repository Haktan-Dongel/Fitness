using System.Collections.Generic;
using System.Threading.Tasks;
using Fitness.Business.Models;

namespace Fitness.Business.Interfaces
{
    public interface IMemberRepository : IGenericRepository<Member>
    {
        Task<IEnumerable<Reservation>> GetReservationsAsync(int memberId);
        Task<IEnumerable<FitnessProgram>> GetProgramsAsync(int memberId);
        Task<IEnumerable<CyclingSession>> GetCyclingSessionsAsync(int memberId);
        Task<IEnumerable<RunningSessionMain>> GetRunningSessionsAsync(int memberId);
    }
}
