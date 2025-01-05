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
        Task<bool> HasConflictingReservationAsync(int memberId, DateTime date, int timeSlotId);
        Task<int> GetDailyReservationCountAsync(int memberId, DateTime date);
        Task<bool> ValidateReservationAsync(int memberId, DateTime date, int timeSlotId, int equipmentId);
        Task<IEnumerable<Reservation>> GetFutureReservationsAsync(int equipmentId);
    }
}
