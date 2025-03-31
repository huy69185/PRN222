using ClassroomBooking.Repository.Entities;

namespace ClassroomBooking.Repository.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Booking> BookingRepository { get; }
        IGenericRepository<Campus> CampusRepository { get; }
        IGenericRepository<Department> DepartmentRepository { get; }
        IGenericRepository<Room> RoomRepository { get; }
        IGenericRepository<RoomSlot> RoomSlotRepository { get; }
        IGenericRepository<Users> UsersRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
