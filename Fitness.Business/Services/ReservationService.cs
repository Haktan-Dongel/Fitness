using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Business.DTOs;
using Fitness.Business.Exceptions;
using Microsoft.Extensions.Logging;

namespace Fitness.Business.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repository;
        private readonly ITimeSlotRepository _timeSlotRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IReservationRepository repository,
            ITimeSlotRepository timeSlotRepository,
            IEquipmentRepository equipmentRepository,
            ILogger<ReservationService> logger)
        {
            _repository = repository;
            _timeSlotRepository = timeSlotRepository;
            _equipmentRepository = equipmentRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ReservationDto>> GetAllReservationsAsync()
        {
            var reservations = await _repository.GetAllAsync();
            return reservations.Select(MapToDto);
        }

        public async Task<ReservationDto> GetReservationAsync(int id)
        {
            var reservation = await _repository.GetByIdAsync(id);
            if (reservation == null)
                throw new NotFoundException($"Reservation {id} not found");
            
            return MapToDto(reservation);
        }

        public async Task<IEnumerable<ReservationDto>> CreateReservationsAsync(CreateReservationDto dto)
        {
            _logger.LogInformation("Starting reservation creation with data: {@Dto}", dto);
            
            var result = new List<ReservationDto>();

            try
            {
                // Valideer de reservering voor elk tijdslot
                foreach (var timeSlotId in dto.TimeSlotIds)
                {
                    var isValid = await ValidateReservationAsync(dto.MemberId, dto.Date, timeSlotId, dto.EquipmentId);
                    _logger.LogInformation("Slot validation result for TimeSlotId {TimeSlotId}: {IsValid}", timeSlotId, isValid);

                    if (!isValid)
                    {
                        throw new ValidationException($"Invalid reservation request for time slot {timeSlotId}");
                    }
                }

                // maak reservation met meerdere timeslots
                var reservationDto = await CreateReservationWithTimeSlotsAsync(dto);
                _logger.LogInformation("Created reservation with ID: {ReservationId}", reservationDto.ReservationId);
                result.Add(reservationDto);

                _logger.LogInformation("Successfully created reservation with IDs: {@ReservationIds}", 
                    result.Select(r => r.ReservationId));
                    
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reservation creation");
                throw;
            }
        }

        private async Task<ReservationDto> CreateReservationWithTimeSlotsAsync(CreateReservationDto dto)
        {
            _logger.LogInformation("Creating reservation with multiple timeslots: {@Dto}", dto);
            
            var timeSlots = new List<TimeSlot>();
            foreach (var timeSlotId in dto.TimeSlotIds)
            {
                var timeSlot = await _timeSlotRepository.GetByIdAsync(timeSlotId);
                if (timeSlot == null)
                {
                    _logger.LogWarning("Invalid time slot ID: {TimeSlotId}", timeSlotId);
                    throw new ValidationException("Invalid time slot");
                }
                timeSlots.Add(timeSlot);
            }

            var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId);
            if (equipment == null)
            {
                _logger.LogWarning("Invalid equipment ID: {EquipmentId}", dto.EquipmentId);
                throw new ValidationException("Invalid equipment");
            }

            var reservation = new Reservation
            {
                MemberId = dto.MemberId,
                EquipmentId = dto.EquipmentId,
                Date = dto.Date,
                Equipment = equipment,
                TimeSlots = timeSlots
            };

            // sla de reservering op
            reservation = await _repository.AddAsync(reservation);
            _logger.LogInformation("Created reservation with ID: {ReservationId}", reservation.ReservationId);

            return MapToDto(reservation);
        }

        public async Task DeleteReservationAsync(int id)
        {
            var reservation = await _repository.GetByIdAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("Attempted to delete non-existent reservation with ID: {Id}", id);
                throw new NotFoundException($"Reservation {id} not found");
            }

            try
            {
                _logger.LogInformation("Deleting reservation {Id} with {Count} time slots", 
                    id, reservation.TimeSlots?.Count ?? 0);
                
                if (reservation.TimeSlots != null)
                {
                    reservation.TimeSlots.Clear();
                }

                await _repository.DeleteAsync(reservation);
                _logger.LogInformation("Successfully deleted reservation {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete reservation {Id}", id);
                throw new InvalidOperationException($"Failed to delete reservation {id}", ex);
            }
        }

        public async Task<bool> ValidateReservationAsync(int memberId, DateTime date, int timeSlotId, int equipmentId)
        {
            // Check dagelijks limiet, max 4 slots per dag
            var dailyCount = await _repository.GetDailyReservationCountAsync(memberId, date);
            if (dailyCount >= 4) 
            {
                _logger.LogWarning("Member {MemberId} has reached daily reservation limit", memberId);
                return false;
            }

            // Check of equipment al gereserveerd is op hetzelfde tijdstip
            var hasConflict = await _repository.HasConflictingReservationAsync(date, timeSlotId, equipmentId);
            if (hasConflict)
            {
                _logger.LogWarning("Conflicting reservation found for equipment {EquipmentId} at time slot {TimeSlotId}", 
                    equipmentId, timeSlotId);
                return false;
            }

            return true;
        }

        public async Task<IEnumerable<ReservationDto>> GetFutureReservationsAsync(int equipmentId)
        {
            var today = DateTime.Today;
            var reservations = await _repository.GetByDateAsync(today);
            return reservations
                .Where(r => r.EquipmentId == equipmentId)
                .Select(MapToDto);
        }

        private static ReservationDto MapToDto(Reservation reservation)
        {
            if (reservation == null) throw new ArgumentNullException(nameof(reservation));

            return new ReservationDto(
                reservation.ReservationId,
                reservation.Date,
                reservation.Equipment?.DeviceType ?? "Unknown Equipment",
                string.Join(", ", reservation.TimeSlots.Select(ts => FormatTimeSlot(ts)))
            );
        }

        private static string FormatTimeSlot(TimeSlot? timeSlot)
        {
            if (timeSlot == null) return "Unknown Time Slot";
            
            return $"{FormatTime(timeSlot.StartTime)} - {FormatTime(timeSlot.EndTime)} ({timeSlot.PartOfDay})";
        }

        private static string FormatTime(int time)
        {
            int hours = time / 100;
            int minutes = time % 100;
            return $"{hours:D2}:{minutes:D2}";
        }

        private async Task<TimeSlot?> GetNextConsecutiveSlotAsync(int currentSlotId)
        {
            var currentSlot = await _timeSlotRepository.GetByIdAsync(currentSlotId);
            if (currentSlot == null) return null;

            return await _timeSlotRepository.GetNextConsecutiveSlotAsync(currentSlot);
        }
    }
}
