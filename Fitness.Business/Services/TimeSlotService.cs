
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Microsoft.Extensions.Logging;

namespace Fitness.Business.Services
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly ITimeSlotRepository _timeSlotRepository;
        private readonly ILogger<TimeSlotService> _logger;

        public TimeSlotService(
            ITimeSlotRepository timeSlotRepository,
            ILogger<TimeSlotService> logger)
        {
            _timeSlotRepository = timeSlotRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<TimeSlot>> GetAllAsync()
        {
            return await _timeSlotRepository.GetAllAsync();
        }

        public async Task<IEnumerable<TimeSlot>> GetByPartOfDayAsync(string partOfDay)
        {
            return await _timeSlotRepository.GetByPartOfDayAsync(partOfDay);
        }

        public async Task<IEnumerable<TimeSlot>> GetAvailableForDateAsync(DateTime date)
        {
            return await _timeSlotRepository.GetAvailableForDateAsync(date);
        }
    }
}