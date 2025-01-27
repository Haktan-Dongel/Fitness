using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fitness.Business.Models;

namespace Fitness.Business.Models
{
    public class Reservation
    {
        [Key]
        [Column("reservation_id")]
        public int ReservationId { get; set; }

        [Column("equipment_id")]
        public int EquipmentId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("member_id")]
        public int MemberId { get; set; }

        public virtual Member Member { get; set; } = null!;
        public virtual Equipment Equipment { get; set; } = null!;
        public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    }
}
