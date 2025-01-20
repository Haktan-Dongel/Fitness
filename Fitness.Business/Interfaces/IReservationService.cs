using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitness.Business.DTOs;

namespace Fitness.Business.Interfaces
{
  public interface IReservationService
  {
    Task<IEnumerable<ReservationDto>> GetAllReservationsAsync();
    Task<ReservationDto> GetReservationAsync(int id);
    Task<IEnumerable<ReservationDto>> CreateReservationsAsync(CreateReservationDto dto);
    Task DeleteReservationAsync(int id);
    Task<bool> ValidateReservationAsync(int memberId, DateTime date, int timeSlotId, int equipmentId);
    Task<IEnumerable<ReservationDto>> GetFutureReservationsAsync(int equipmentId);
  }
}