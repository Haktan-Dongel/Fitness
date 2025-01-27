using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Fitness.Business.Interfaces;
using Fitness.Business.DTOs;
using Fitness.Business.Exceptions;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IReservationService reservationService,
            ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllReservations()
        {
            try
            {
                var reservations = await _reservationService.GetAllReservationsAsync();
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations");
                return StatusCode(500, "Error retrieving reservations");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDto>> GetReservation(int id)
        {
            try
            {
                var reservation = await _reservationService.GetReservationAsync(id);
                return Ok(reservation);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation {ReservationId}", id);
                return StatusCode(500, "Error retrieving reservation");
            }
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create new reservations",
            Description = "Creates one or two consecutive reservations based on IncludeNextSlot flag"
        )]
        [ProducesResponseType(typeof(IEnumerable<ReservationDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> CreateReservation([FromBody] CreateReservationDto dto)
        {
            try
            {
                _logger.LogInformation("Starting reservation creation with data: {@Dto}", dto);

                if (dto == null)
                {
                    _logger.LogWarning("Received null reservation DTO");
                    return BadRequest("Reservation data is required");
                }

                var reservations = await _reservationService.CreateReservationsAsync(dto);
                
                _logger.LogInformation("Successfully created {Count} reservations", reservations.Count());
                
                var firstReservation = reservations.First();
                return CreatedAtAction(
                    nameof(GetReservation), 
                    new { id = firstReservation.ReservationId }, 
                    reservations);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed for reservation request: {@RequestDto}", dto);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservations for request: {@RequestDto}", dto);
                return StatusCode(500, "Error creating reservations");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            try
            {
                await _reservationService.DeleteReservationAsync(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reservation {ReservationId}", id);
                return StatusCode(500, "Error deleting reservation");
            }
        }
    }
}
