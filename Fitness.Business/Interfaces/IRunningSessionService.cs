using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitness.Business.Models;

namespace Fitness.Business.Interfaces
{
    public interface IRunningSessionService
    {
        Task<IEnumerable<RunningSessionMain>> GetAllForMemberAsync(int memberId);
        Task<IEnumerable<RunningSessionMain>> GetByMonthAsync(int memberId, int year, int month);
        Task<IEnumerable<RunningSessionMain>> GetByDateRangeAsync(int memberId, DateTime startDate, DateTime endDate);
    }
}
