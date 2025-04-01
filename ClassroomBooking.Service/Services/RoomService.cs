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
        }

        public async Task UpdateRoomAsync(Room room, bool resetCapacity = false, int additionalSeats = 0)
        {
            var existingRoom = await _unitOfWork.RoomRepository.GetByIdAsync(room.RoomId);
            if (existingRoom == null) throw new Exception("Room not found.");

            // Nếu chuyển từ Occupied sang Available thì reset capacity hoặc thêm số ghế theo tùy chọn
            if (existingRoom.Status == "Occupied" && room.Status == "Available")
            {
                if (resetCapacity)
                {
                    existingRoom.Capacity = room.Capacity;
                }
                else if (additionalSeats > 0)
                {
                    existingRoom.Capacity += additionalSeats;
                }
                else
                {
                    throw new Exception("Cannot update from Occupied to Available without resetting or adding seats.");
                }
            }

            // Cập nhật các thuộc tính khác
            existingRoom.RoomName = room.RoomName;
            existingRoom.CampusId = room.CampusId;
            existingRoom.Status = room.Status;
            existingRoom.Description = room.Description;

            _unitOfWork.RoomRepository.Update(existingRoom);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteRoomAsync(int roomId)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room == null) throw new Exception("Room not found.");
            // Khi xóa, chuyển trạng thái của phòng sang "Maintenance" (do Manager thiết lập)
            room.Status = "Maintenance";
            _unitOfWork.RoomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<Room>> GetRoomsByCampusAsync(int campusId)
        {
            // Lấy tất cả phòng thuộc campus, loại trừ những phòng có trạng thái "Maintenance"
            return await _unitOfWork.RoomRepository.GetAllAsync(
                r => r.CampusId == campusId && r.Status != "Maintenance", "Campus");
        }
    }
}
