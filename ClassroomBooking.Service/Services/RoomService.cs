using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Interfaces;

namespace ClassroomBooking.Service
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Room?> GetRoomByIdAsync(int roomId)
        {
            return await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
        }

        public async Task<List<Room>> GetAllRoomsAsync()
        {
            return await _unitOfWork.RoomRepository.GetAllAsync(includeProperties: "Campus");
        }

        public async Task CreateRoomAsync(Room room)
        {
            await _unitOfWork.RoomRepository.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();
            await UpdateRoomStatusBasedOnCapacityAsync(room.RoomId, room.Capacity);
        }

        public async Task UpdateRoomAsync(Room room)
        {
            var existingRoom = await _unitOfWork.RoomRepository.GetByIdAsync(room.RoomId);
            if (existingRoom == null) throw new Exception("Room not found.");

            existingRoom.RoomName = room.RoomName;
            existingRoom.CampusId = room.CampusId;
            existingRoom.Capacity = room.Capacity;
            existingRoom.Status = room.Status;
            existingRoom.Description = room.Description;

            _unitOfWork.RoomRepository.Update(existingRoom);
            await _unitOfWork.SaveChangesAsync();
            await UpdateRoomStatusBasedOnCapacityAsync(room.RoomId, room.Capacity);
        }

        public async Task DeleteRoomAsync(int roomId)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room == null) throw new Exception("Room not found.");
            room.Status = "Maintenance";
            _unitOfWork.RoomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<Room>> GetRoomsByCampusAsync(int campusId)
        {
            return await _unitOfWork.RoomRepository.GetAllAsync(
                r => r.CampusId == campusId && r.Status == "Available", "Campus");
        }

        public async Task UpdateRoomStatusBasedOnCapacityAsync(int roomId, int capacityLeft)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room == null) throw new Exception("Room not found.");

            room.Status = capacityLeft > 0 ? "Available" : "Occupied";
            _unitOfWork.RoomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}