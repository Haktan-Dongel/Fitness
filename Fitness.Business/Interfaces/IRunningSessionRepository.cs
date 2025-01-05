using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitness.Business.Models;

namespace Fitness.Business.Interfaces
{
    public interface IRunningSessionRepository : IGenericRepository<RunningSessionMain>
    {
        Task<RunningSessionMain> GetWithDetailsAsync(int runningSessionId);
        Task<IEnumerable<RunningSessionMain>> GetByMemberAsync(int memberId);
        Task<IEnumerable<RunningSessionMain>> GetByDateRangeAsync(int memberId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<RunningSessionMain>> GetByMonthAsync(int memberId, int year, int month);
        Task AddDetailsAsync(int runningSessionId, IEnumerable<RunningSessionDetail> details);
        Task<(double TotalDistance, double AverageSpeed)> GetPerformanceStatsAsync(int memberId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<RunningSessionMain>> GetAllForMemberAsync(int memberId);
    }
}
