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
        Task<IEnumerable<CyclingSession>> GetByMonthAsync(int memberId, int year, int month);
        Task<(double TotalWatt, double MaxWatt)> GetPerformanceStatsAsync(int memberId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<CyclingSession>> GetAllForMemberAsync(int memberId);
    }
}
