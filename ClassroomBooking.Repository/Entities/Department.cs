namespace ClassroomBooking.Repository.Entities
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public int CampusId { get; set; } 
        public Campus Campus { get; set; } = null!; 
        public List<Users> Users { get; set; } = new List<Users>(); 
    }
}