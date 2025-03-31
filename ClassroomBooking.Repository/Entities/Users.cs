using System.ComponentModel.DataAnnotations;

namespace ClassroomBooking.Repository.Entities
{
    public partial class Users
    {
        public int USerId { get; set; }
        [Required]
        [StringLength(20)]
        public string UserCode { get; set; } = null!;
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [StringLength(100)]
        public string Password { get; set; } = null!;
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Student"; // "Student" hoặc "Manager"
        [Required]
        public int DepartmentId { get; set; }
        [Required]
        public int CampusId { get; set; }

        public virtual Department Department { get; set; } = null!;
        public virtual Campus Campus { get; set; } = null!;
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}