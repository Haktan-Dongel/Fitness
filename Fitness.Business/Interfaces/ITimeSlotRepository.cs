using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitness.Business.Models;

namespace Fitness.Business.Interfaces
{
    public interface ITimeSlotRepository : IGenericRepository<TimeSlot>
    {
        Task<IEnumerable<TimeSlot>> GetByPartOfDayAsync(string partOfDay);
        Task<TimeSlot?> GetByStartAndEndTimeAsync(int startTime, int endTime);
        Task<IEnumerable<TimeSlot>> GetAvailableForDateAsync(DateTime date);
        Task<bool> IsTimeSlotAvailableAsync(int timeSlotId, DateTime date);
        Task<TimeSlot?> GetNextConsecutiveSlotAsync(TimeSlot currentSlot);
    }
}
