using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Fitness.Business.Models;
using Fitness.Data.Context;
using Fitness.Business.Interfaces;

namespace Fitness.Data.Repositories
{
    public class EquipmentRepository : GenericRepository<Equipment>, IEquipmentRepository
    {
        public EquipmentRepository(FitnessContext context) : base(context) { }

        public async Task<bool> IsAvailableForTimeSlotAsync(int equipmentId, int timeSlotId, DateTime date)
        {
            var equipment = await _dbSet.FindAsync(equipmentId);
            if (equipment == null)
                return false;

            return !await _context.Reservations
                .AnyAsync(r => r.EquipmentId == equipmentId 
                           && r.TimeSlots.Any(ts => ts.TimeSlotId == timeSlotId) 
                           && r.Date.Date == date.Date);
        }

        public async Task<IEnumerable<Equipment>> GetAvailableEquipmentForTimeSlotAsync(int timeSlotId, DateTime date)
        {
            var reservedEquipmentIds = await _context.Reservations
                .Where(r => r.TimeSlots.Any(ts => ts.TimeSlotId == timeSlotId) && r.Date.Date == date.Date)
                .Select(r => r.EquipmentId)
                .ToListAsync();

            return await _dbSet
                .Where(e => !reservedEquipmentIds.Contains(e.EquipmentId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Equipment>> GetByTypeAsync(string deviceType)
        {
            return await _dbSet
                .Where(e => e.DeviceType == deviceType)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Equipment>> GetAllAsync()
        {
            return await _dbSet
                .Include(e => e.Reservations)
                .ThenInclude(r => r.TimeSlots)
                .ToListAsync();
        }
    }
}
