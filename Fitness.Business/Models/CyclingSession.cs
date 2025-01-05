using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Fitness.Business.Models
{
    public class CyclingSession
    {
        [Key]
        public int CyclingSessionId { get; set; }

        [Column("member_id")]
        public int MemberId { get; set; }

        public DateTime Date { get; set; }
        public int Duration { get; set; }

        [Column("avg_watt")]
        public int AvgWatt { get; set; }

        [Column("max_watt")]
        public int MaxWatt { get; set; }

        public int AvgCadence { get; set; }
        public int MaxCadence { get; set; }
        public string TrainingType { get; set; } = string.Empty;
        public string? Comment { get; set; }

        [JsonIgnore]
        public virtual Member Member { get; set; } = null!;
    }

    public enum TrainingType
    {
        Fun,
        Endurance,
        Interval,
        Recovery
    }
}
