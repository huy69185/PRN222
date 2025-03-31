using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Interfaces;

namespace ClassroomBooking.Service
{
    public class UsersService : IUsersService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Users?> GetUserAsync(string userCode)
        {
            return await _unitOfWork.UsersRepository.GetFirstOrDefaultAsync(u => u.UserCode == userCode, "Department,Campus");
        }

        public async Task RegisterUserAsync(Users user)
        {
            if (string.IsNullOrEmpty(user.UserCode) || string.IsNullOrEmpty(user.FullName) ||
                string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                throw new ArgumentException("All fields are required.");

            var existed = await GetUserAsync(user.UserCode);
            if (existed != null) throw new Exception("UserCode already exists!");

            if (user.Role == "Manager")
            {
                var existingManager = await _unitOfWork.UsersRepository.GetFirstOrDefaultAsync(
                    u => u.CampusId == user.CampusId && u.Role == "Manager");
                if (existingManager != null)
                    throw new Exception("This campus already has a Manager.");
            }

            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(user.DepartmentId);
            if (department == null) throw new Exception("Invalid DepartmentId.");

            // Kiểm tra Department và CampusId có khớp không
            if (department.CampusId != user.CampusId)
                throw new Exception("Department does not belong to the selected campus.");

            if (user.Role != "Student" && user.Role != "Manager") user.Role = "Student";

            await _unitOfWork.UsersRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Users> LoginAsync(string userCode, string password)
        {
            var user = await GetUserAsync(userCode);
            if (user == null) throw new Exception("User not found!");
            if (user.Password != password) throw new Exception("Invalid password!");
            return user;
        }
    }
}