using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Interfaces;

namespace ClassroomBooking.Service
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            return await _unitOfWork.DepartmentRepository.GetAllAsync();
        }

        public async Task<Department?> GetDepartmentByIdAsync(int id)
        {
            return await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
        }
    }
}