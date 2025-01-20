using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
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
            _logger.LogInformation("Creating reservation(s) for {@Dto}", dto);
            var result = new List<ReservationDto>();

            // valideer en creëer eerste reservering
            if (!await ValidateReservationAsync(dto.MemberId, dto.Date, dto.TimeSlotId, dto.EquipmentId))
            {
                throw new ValidationException("Invalid reservation request");
            }

            var firstReservation = await CreateSingleReservationAsync(dto);
            result.Add(firstReservation);

            // creëer tweede reservering indien gewenst
            if (dto.IncludeNextSlot)
            {
                var nextSlot = await GetNextConsecutiveSlotAsync(dto.TimeSlotId);
                if (nextSlot != null && await ValidateReservationAsync(dto.MemberId, dto.Date, nextSlot.TimeSlotId, dto.EquipmentId))
                {
                    var nextReservation = await CreateSingleReservationAsync(new CreateReservationDto
                    {
                        MemberId = dto.MemberId,
                        EquipmentId = dto.EquipmentId,
                        TimeSlotId = nextSlot.TimeSlotId,
                        Date = dto.Date
                    });
                    result.Add(nextReservation);
                }
            }

            return result;
        }

        private async Task<ReservationDto> CreateSingleReservationAsync(CreateReservationDto dto)
        {
            var reservation = new Reservation
            {
                MemberId = dto.MemberId,
                EquipmentId = dto.EquipmentId,
                TimeSlotId = dto.TimeSlotId,
                Date = dto.Date
            };

            var createdReservation = await _repository.AddAsync(reservation);
            var equipment = await _equipmentRepository.GetByIdAsync(createdReservation.EquipmentId);
            var timeSlot = await _timeSlotRepository.GetByIdAsync(createdReservation.TimeSlotId);
        
            createdReservation.Equipment = equipment;
            createdReservation.TimeSlot = timeSlot;

            return MapToDto(createdReservation);
        }

        public async Task DeleteReservationAsync(int id)
        {
            var reservation = await _repository.GetByIdAsync(id);
            if (reservation == null)
                throw new NotFoundException($"Reservation {id} not found");

            await _repository.DeleteAsync(reservation);
        }

        public async Task<bool> ValidateReservationAsync(int memberId, DateTime date, int timeSlotId, int equipmentId)
        {
            // check dagelijkse limiet (max 4 slots per dag)
            var dailyCount = await _repository.GetDailyReservationCountAsync(memberId, date);
            if (dailyCount >= 4) 
            {
                _logger.LogWarning("Member {MemberId} has reached daily reservation limit", memberId);
                return false;
            }

            // check of equipment al gereserveerd is voor het tijdslot
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
                FormatTimeSlot(reservation.TimeSlot)
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
