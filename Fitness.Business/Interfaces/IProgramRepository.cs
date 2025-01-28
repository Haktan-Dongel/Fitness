using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitness.Business.Models;

namespace Fitness.Business.Interfaces
{
    public interface IProgramRepository : IGenericRepository<FitnessProgram>
    {
        Task<int> GetCurrentMemberCountAsync(string programCode);
        Task<FitnessProgram?> GetByProgramCodeAsync(string programCode);
        Task<IEnumerable<FitnessProgram>> GetActiveAsync(DateTime date);
        Task<bool> IsMemberEnrolledAsync(string programCode, int memberId);
        Task<IEnumerable<Member>> GetProgramMembersAsync(string programCode);
        Task AddMemberToProgramAsync(string programCode, int memberId);
    }
}
