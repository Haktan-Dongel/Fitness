using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Data.Context;

namespace Fitness.Data.Repositories
{
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        public ReservationRepository(FitnessContext context) : base(context) { }

        public async Task<IEnumerable<Reservation>> GetByMemberAsync(int memberId)
        {
            return await _dbSet
                .Include(r => r.Equipment)
                .Include(r => r.TimeSlots)
                .Where(r => r.MemberId == memberId)
                .OrderBy(r => r.Date)
                .ThenBy(r => r.TimeSlots.Min(ts => ts.StartTime))
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByDateAsync(DateTime date)
        {
            return await _dbSet
                .Include(r => r.Equipment)
                .Include(r => r.TimeSlots)
                .Where(r => r.Date.Date == date.Date)
                .OrderBy(r => r.TimeSlots.Min(ts => ts.StartTime))
                .ToListAsync();
        }

        public async Task<bool> HasConflictingReservationAsync(int memberId, DateTime date, int timeSlotId)
        {
            return await _dbSet
                .AnyAsync(r => r.MemberId == memberId 
                           && r.Date.Date == date.Date 
                           && r.TimeSlots.Any(ts => ts.TimeSlotId == timeSlotId));
        }

        public async Task<int> GetDailyReservationCountAsync(int memberId, DateTime date)
        {
            return await _dbSet
                .CountAsync(r => r.MemberId == memberId && r.Date.Date == date.Date);
        }

        public async Task<bool> ValidateReservationAsync(int memberId, DateTime date, int timeSlotId, int equipmentId)
        {
            // Check daily limit (max 4 slots per day)
            var dailyCount = await GetDailyReservationCountAsync(memberId, date);
            if (dailyCount >= 4) return false;

            // Check if equipment is already reserved
            var isEquipmentAvailable = !await _dbSet
                .AnyAsync(r => r.EquipmentId == equipmentId 
                           && r.Date.Date == date.Date 
                           && r.TimeSlots.Any(ts => ts.TimeSlotId == timeSlotId));

            return isEquipmentAvailable;
        }

        public async Task<IEnumerable<Reservation>> GetFutureReservationsAsync(int equipmentId)
        {
            return await _dbSet
                .Include(r => r.TimeSlots)
                .Where(r => r.EquipmentId == equipmentId && r.Date.Date >= DateTime.Today)
                .OrderBy(r => r.Date)
                .ThenBy(r => r.TimeSlots.Min(ts => ts.StartTime))
                .ToListAsync();
        }

        public async Task<bool> HasConflictingReservationAsync(DateTime date, int timeSlotId, int equipmentId)
        {
            return await _dbSet
                .AnyAsync(r => r.Date.Date == date.Date 
                           && r.TimeSlots.Any(ts => ts.TimeSlotId == timeSlotId)
                           && r.EquipmentId == equipmentId);
        }

        public async Task<IEnumerable<Reservation>> GetByDateAndTimeSlotAsync(DateTime date, int timeSlotId)
        {
            return await _dbSet
                .Include(r => r.Equipment)
                .Include(r => r.TimeSlots)
                .Where(r => r.Date.Date == date.Date && r.TimeSlots.Any(ts => ts.TimeSlotId == timeSlotId))
                .ToListAsync();
        }
    }
}
