using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Fitness.Business.Models
{
    [Table("members")]
    public class Member
    {
        [Key]
        [Column("member_id")]
        public int MemberId { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Column("email")]
        public string? Email { get; set; }

        [Column("address")]
        public string Address { get; set; } = string.Empty;

        [Column("birthday")]
        public DateTime Birthday { get; set; }

        [Column("interests")]
        public string? Interests { get; set; }

        [Column("membertype")]
        public string? MemberType { get; set; }

        public virtual ICollection<RunningSessionMain> RunningSessions { get; set; } = new List<RunningSessionMain>();
        public virtual ICollection<CyclingSession> CyclingSessions { get; set; } = new List<CyclingSession>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<ProgramMembers> ProgramMemberships { get; set; } = new List<ProgramMembers>();
    }
}
