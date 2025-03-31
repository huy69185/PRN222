using System.ComponentModel.DataAnnotations;

namespace ClassroomBooking.Repository.Entities
{
    public partial class Room
    {
        public int RoomId { get; set; }
        [Required]
        [StringLength(100)]
        public string RoomName { get; set; } = null!;
        [Required]
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
        [Required]
        public int CampusId { get; set; }
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Available";
        [StringLength(255)]
        public string? Description { get; set; }

        public virtual Campus Campus { get; set; } = null!;
        public virtual ICollection<RoomSlot> RoomSlots { get; set; } = new List<RoomSlot>();
    }
}