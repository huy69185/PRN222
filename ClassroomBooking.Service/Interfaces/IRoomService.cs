using ClassroomBooking.Repository.Entities;


namespace ClassroomBooking.Service.Interfaces
{
    public interface IRoomService
    {
        Task<Room?> GetRoomByIdAsync(int roomId);
        Task<List<Room>> GetAllRoomsAsync();
        Task CreateRoomAsync(Room room);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(int roomId);
        Task<List<Room>> GetRoomsByCampusAsync(int campusId);
        Task UpdateRoomStatusBasedOnCapacityAsync(int roomId, int capacityLeft);
    }
}
