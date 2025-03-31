using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Interfaces;

namespace ClassroomBooking.Service
{
    public class CampusService : ICampusService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CampusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Campus>> GetAllCampusesAsync()
        {
            return await _unitOfWork.CampusRepository.GetAllAsync();
        }
    }
}