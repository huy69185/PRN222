using System.ComponentModel.DataAnnotations;

namespace ClassroomBooking.Repository.Entities
{
    public partial class Campus
    {
        public int CampusId { get; set; }
        [Required]
        [StringLength(100)]
        public string CampusName { get; set; } = null!;
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}