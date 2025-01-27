using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness.Business.Models
{
    public class TimeSlot
    {
        [Key]
        [Column("time_slot_id")]
        public int TimeSlotId { get; set; }

        [Column("start_time")]
        public int StartTime { get; set; }

        [Column("end_time")]
        public int EndTime { get; set; }

        [Column("part_of_day")]
        public string PartOfDay { get; set; } = string.Empty;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public string DisplayText => $"{FormatTime(StartTime)} - {FormatTime(EndTime)} ({PartOfDay})";

        private static string FormatTime(int time)
        {
            int hours = time / 100;
            int minutes = time % 100;
            return $"{hours:D2}:{minutes:D2}";
        }
    }
}
