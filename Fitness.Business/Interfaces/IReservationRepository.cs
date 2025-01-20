using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitness.Business.Models;

namespace Fitness.Business.Interfaces
{
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        Task<IEnumerable<Reservation>> GetByMemberAsync(int memberId);
        Task<IEnumerable<Reservation>> GetByDateAsync(DateTime date);
        Task<bool> HasConflictingReservationAsync(DateTime date, int timeSlotId, int equipmentId);
        Task<int> GetDailyReservationCountAsync(int memberId, DateTime date);
    }
}
