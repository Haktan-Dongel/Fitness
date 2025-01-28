using Microsoft.AspNetCore.Mvc;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeSlotsController : ControllerBase
    {
        private readonly ITimeSlotService _timeSlotService;
        private readonly ILogger<TimeSlotsController> _logger;

        public TimeSlotsController(
            ITimeSlotService timeSlotService,
            ILogger<TimeSlotsController> logger)
        {
            _timeSlotService = timeSlotService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeSlot>>> GetAllTimeSlots()
        {
            try
            {
                var timeSlots = await _timeSlotService.GetAllAsync();
                return Ok(timeSlots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving time slots");
                return StatusCode(500, "Error retrieving time slots");
            }
        }

        [HttpGet("part-of-day/{partOfDay}")]
        public async Task<ActionResult<IEnumerable<TimeSlot>>> GetByPartOfDay(string partOfDay)
        {
            try
            {
                var timeSlots = await _timeSlotService.GetByPartOfDayAsync(partOfDay);
                return Ok(timeSlots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving time slots for part of day: {PartOfDay}", partOfDay);
                return StatusCode(500, "Error retrieving time slots");
            }
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<TimeSlot>>> GetAvailableTimeSlots([FromQuery] DateTime date)
        {
            try
            {
                var timeSlots = await _timeSlotService.GetAvailableForDateAsync(date);
                return Ok(timeSlots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available time slots for date: {Date}", date);
                return StatusCode(500, "Error retrieving available time slots");
            }
        }
    }
}
