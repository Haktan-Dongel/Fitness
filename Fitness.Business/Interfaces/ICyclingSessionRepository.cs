using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitness.Business.Models;

namespace Fitness.Business.Interfaces
{
    public interface ICyclingSessionRepository : IGenericRepository<CyclingSession>
    {
        Task<IEnumerable<CyclingSession>> GetByMemberAsync(int memberId);
        Task<IEnumerable<CyclingSession>> GetByDateRangeAsync(int memberId, DateTime startDate, DateTime endDate);
    }
}
