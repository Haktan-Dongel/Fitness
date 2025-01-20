using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Data.Context;

namespace Fitness.Data.Repositories
{
    public class TimeSlotRepository : GenericRepository<TimeSlot>, ITimeSlotRepository
    {
        public TimeSlotRepository(FitnessContext context) : base(context) { }

        public async Task<IEnumerable<TimeSlot>> GetByPartOfDayAsync(string partOfDay)
        {
            return await _dbSet
                .Where(ts => ts.PartOfDay == partOfDay)
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();
        }

        public async Task<TimeSlot?> GetByStartAndEndTimeAsync(int startTime, int endTime)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ts => ts.StartTime == startTime && ts.EndTime == endTime);
        }

        public async Task<IEnumerable<TimeSlot>> GetAvailableForDateAsync(DateTime date)
        {
            return await _dbSet
                .Where(ts => !_context.Reservations
                    .Any(r => r.TimeSlotId == ts.TimeSlotId && r.Date.Date == date.Date))
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int timeSlotId, DateTime date)
        {
            return !await _context.Reservations
                .AnyAsync(r => r.TimeSlotId == timeSlotId && r.Date.Date == date.Date);
        }

        public async Task<TimeSlot?> GetNextConsecutiveSlotAsync(TimeSlot currentSlot)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ts => ts.StartTime == currentSlot.EndTime);
        }
    }
}
