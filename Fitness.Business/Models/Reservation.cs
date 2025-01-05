using Fitness.Business.Models;

namespace Fitness.Business.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }         
        public int EquipmentId { get; set; }         
        public int TimeSlotId { get; set; }       
        public DateTime Date { get; set; }          
        public int MemberId { get; set; }         

        public Member Member { get; set; } = null!;
        public Equipment Equipment { get; set; } = null!;
        public TimeSlot TimeSlot { get; set; } = null!;
    }
}
