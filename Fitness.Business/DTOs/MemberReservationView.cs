using System;
using System.Collections.Generic;

namespace Fitness.Business.DTOs
{
    public class MemberReservationView
    {
        public int ReservationId { get; set; }
        public DateTime Date { get; set; }
        public string Equipment { get; set; } = string.Empty;
        public List<string> TimeSlots { get; set; } = new List<string>();
    }
}