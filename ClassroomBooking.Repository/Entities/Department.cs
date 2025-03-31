namespace ClassroomBooking.Repository.Entities
{
    public partial class Department
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public virtual ICollection<Users> Users { get; set; } = new List<Users>();
    }
}