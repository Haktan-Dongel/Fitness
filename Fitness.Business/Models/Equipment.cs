namespace Fitness.Business.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }
        public string DeviceType { get; set; } = string.Empty;
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
