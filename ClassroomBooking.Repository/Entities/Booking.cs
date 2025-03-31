using System.ComponentModel.DataAnnotations;

namespace ClassroomBooking.Repository.Entities
{
    public partial class Booking
    {
        public int BookingId { get; set; }
        [Required]
        public int StudentId { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        [StringLength(50)]
        public string BookingStatus { get; set; } = "Pending";
        [Required]
        public string Purpose { get; set; } = null!;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual Users User { get; set; } = null!;
        public virtual ICollection<RoomSlot> RoomSlots { get; set; } = new List<RoomSlot>();
    }
}