using Microsoft.AspNetCore.Mvc;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ILogger<EquipmentController> _logger;

        public EquipmentController(IEquipmentRepository equipmentRepository, ILogger<EquipmentController> logger)
        {
            _equipmentRepository = equipmentRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetAllEquipment()
        {
            try
            {
                var equipment = await _equipmentRepository.GetAllAsync();
                return Ok(equipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment");
                return StatusCode(500, "Error retrieving equipment");
            }
        }

        [HttpGet("type/{deviceType}")]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetByType(string deviceType)
        {
            try
            {
                var equipment = await _equipmentRepository.GetByTypeAsync(deviceType);
                return Ok(equipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment by type");
                return StatusCode(500, "Error retrieving equipment");
            }
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetAvailableEquipment(
            [FromQuery] int timeSlotId,
            [FromQuery] DateTime date)
        {
            try
            {
                var equipment = await _equipmentRepository.GetAvailableEquipmentForTimeSlotAsync(timeSlotId, date);
                return Ok(equipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available equipment");
                return StatusCode(500, "Error retrieving available equipment");
            }
        }

        [HttpGet("check-availability")]
        public async Task<ActionResult<bool>> CheckAvailability(
            [FromQuery] int equipmentId,
            [FromQuery] int timeSlotId,
            [FromQuery] DateTime date)
        {
            try
            {
                var isAvailable = await _equipmentRepository.IsAvailableForTimeSlotAsync(equipmentId, timeSlotId, date);
                return Ok(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking equipment availability");
                return StatusCode(500, "Error checking equipment availability");
            }
        }
    }
}
