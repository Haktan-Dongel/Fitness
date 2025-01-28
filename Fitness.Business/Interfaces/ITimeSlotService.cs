
using Fitness.Business.Models;

namespace Fitness.Business.Interfaces
{
    public interface ITimeSlotService
    {
        Task<IEnumerable<TimeSlot>> GetAllAsync();
        Task<IEnumerable<TimeSlot>> GetByPartOfDayAsync(string partOfDay);
        Task<IEnumerable<TimeSlot>> GetAvailableForDateAsync(DateTime date);
    }
}