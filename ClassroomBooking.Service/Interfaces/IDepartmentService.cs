using ClassroomBooking.Repository.Entities;

namespace ClassroomBooking.Service.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllDepartmentsAsync();
        Task<Department?> GetDepartmentByIdAsync(int id);
    }
}
