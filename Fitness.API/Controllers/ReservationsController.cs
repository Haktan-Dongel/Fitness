using Microsoft.AspNetCore.Mvc;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Business.DTOs;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IReservationRepository reservationRepository,
            IEquipmentRepository equipmentRepository,
            ILogger<ReservationsController> logger)
        {
            _reservationRepository = reservationRepository;
            _equipmentRepository = equipmentRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetAllReservations()
        {
            try
            {
                var reservations = await _reservationRepository.GetAllAsync();
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations");
                return StatusCode(500, "Error retrieving reservations");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null)
                    return NotFound();

                return Ok(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation {ReservationId}", id);
                return StatusCode(500, "Error retrieving reservation");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Reservation>> CreateReservation([FromBody] CreateReservationDto dto)
        {
            try
            {
                _logger.LogInformation("Received reservation request: {@RequestDto}", dto);

                var reservation = new Reservation
                {
                    MemberId = dto.MemberId,
                    EquipmentId = dto.EquipmentId,
                    TimeSlotId = dto.TimeSlotId,
                    Date = dto.Date.Date 
                };

                // Basis validatie
                if (dto.EquipmentId <= 0 || dto.TimeSlotId <= 0 || dto.MemberId <= 0)
                {
                    return BadRequest("Invalid Equipment, TimeSlot, or Member ID");
                }

                // Valideer datum niet in het verleden
                var today = DateTime.Today;
                var requestDate = dto.Date.Date;
                
                if (requestDate < today)
                {
                    return BadRequest("Cannot create reservations for past dates");
                }

                // Valideer datum niet meer dan een week in de toekomst
                var maxDate = today.AddDays(7);
                if (requestDate > maxDate)
                {
                    return BadRequest("Cannot create reservations more than one week in advance");
                }

                // Valideer dagelijkse limiet (max 4)
                var dailyCount = await _reservationRepository.GetDailyReservationCountAsync(dto.MemberId, requestDate);
                if (dailyCount >= 4)
                {
                    return BadRequest("Maximum daily reservations (4) reached");
                }

                // valideer max 2 aaneengesloten tijdsloten
                var memberReservations = await _reservationRepository.GetByMemberAsync(dto.MemberId);
                var sameDayReservations = memberReservations
                    .Where(r => r.Date.Date == requestDate)
                    .OrderBy(r => r.TimeSlotId)
                    .ToList();

                if (sameDayReservations.Any())
                {
                    var consecutiveCount = 1;
                    var lastSlotId = sameDayReservations.First().TimeSlotId;

                    foreach (var res in sameDayReservations.Skip(1))
                    {
                        if (res.TimeSlotId == lastSlotId + 1)
                        {
                            consecutiveCount++;
                            if (consecutiveCount >= 2 && dto.TimeSlotId == lastSlotId + 1)
                            {
                                return BadRequest("Cannot reserve more than 2 consecutive time slots");
                            }
                        }
                        else
                        {
                            consecutiveCount = 1;
                        }
                        lastSlotId = res.TimeSlotId;
                    }
                }

                // valideer beschikbaarheid van apparatuur
                var isAvailable = await _equipmentRepository.IsAvailableForTimeSlotAsync(
                    dto.EquipmentId, dto.TimeSlotId, requestDate);

                if (!isAvailable)
                {
                    return BadRequest("Equipment is not available for the selected time slot");
                }

                // Maak de reservering
                await _reservationRepository.AddAsync(reservation);
                await _reservationRepository.SaveChangesAsync();

                return CreatedAtAction(nameof(GetReservation), 
                    new { id = reservation.ReservationId }, reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                return StatusCode(500, "Error creating reservation");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null)
                    return NotFound();

                await _reservationRepository.DeleteAsync(reservation);
                await _reservationRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reservation {ReservationId}", id);
                return StatusCode(500, "Error deleting reservation");
            }
        }
    }
}
