using System;

namespace Fitness.Business.DTOs
{
    public class CreateReservationDto
    {
        public int MemberId { get; set; }
        public int EquipmentId { get; set; }
        public int TimeSlotId { get; set; }
        public DateTime Date { get; set; }
    }
}
