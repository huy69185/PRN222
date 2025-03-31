using ClassroomBooking.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassroomBooking.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ClassroomBookingDbContext _context;
        private IGenericRepository<Booking>? _bookingRepository;
        private IGenericRepository<Campus>? _campusRepository;
        private IGenericRepository<Department>? _departmentRepository;
        private IGenericRepository<Room>? _roomRepository;
        private IGenericRepository<RoomSlot>? _roomSlotRepository;
        private IGenericRepository<Users>? _usersRepository;

        public UnitOfWork(ClassroomBookingDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Booking> BookingRepository
            => _bookingRepository ??= new GenericRepository<Booking>(_context);
        public IGenericRepository<Campus> CampusRepository
            => _campusRepository ??= new GenericRepository<Campus>(_context);
        public IGenericRepository<Department> DepartmentRepository
            => _departmentRepository ??= new GenericRepository<Department>(_context);
        public IGenericRepository<Room> RoomRepository
            => _roomRepository ??= new GenericRepository<Room>(_context);
        public IGenericRepository<RoomSlot> RoomSlotRepository
            => _roomSlotRepository ??= new GenericRepository<RoomSlot>(_context);
        public IGenericRepository<Users> UsersRepository
            => _usersRepository ??= new GenericRepository<Users>(_context);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}
