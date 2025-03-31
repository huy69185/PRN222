using ClassroomBooking.Repository.Entities;


namespace ClassroomBooking.Service.Interfaces
{
    public interface ICampusService
    {
        Task<List<Campus>> GetAllCampusesAsync();
    }
}
