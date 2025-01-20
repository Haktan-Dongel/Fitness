using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fitness.Business.DTOs
{
    public class CreateReservationDto
    {

        [Required]
        public int MemberId { get; set; }

        [Required]
        public int EquipmentId { get; set; }

        [Required]
        public int TimeSlotId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IncludeNextSlot { get; set; }
    }
}
