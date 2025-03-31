using ClassroomBooking.Repository.Entities;

namespace ClassroomBooking.Service.Interfaces
{
    public interface IUsersService
    {
        Task<Users?> GetUserAsync(string userCode);
        Task RegisterUserAsync(Users user);
        Task<Users> LoginAsync(string userCode, string password);
    }
}
