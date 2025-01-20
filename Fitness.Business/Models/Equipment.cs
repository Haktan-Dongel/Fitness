using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness.Business.Models
{
    public class Equipment
    {
        [Key]
        [Column("equipment_id")]
        public int EquipmentId { get; set; }

        [Required]
        [Column("device_type")]
        [StringLength(45)]
        public string DeviceType { get; set; } = string.Empty;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
