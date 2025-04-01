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

            // Kiểm tra nếu chuyển từ Occupied sang Available
            if (existingRoom.Status == "Occupied" && room.Status == "Available")
            {
                if (resetCapacity)
                {
                    // Reset về dung lượng ban đầu (giả sử room.Capacity là giá trị ban đầu)
                    existingRoom.Capacity = room.Capacity;
                }
                else if (additionalSeats > 0)
                {
                    // Thêm số ghế vào Capacity hiện tại
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

            // Cập nhật trạng thái dựa trên capacityLeft
            if (capacityLeft > 0)
            {
                room.Status = "Available";
            }
            else if (capacityLeft == 0)
            {
                room.Status = "Occupied";
            }
            else
            {
                // Nếu capacityLeft < 0 (trường hợp bất thường), đặt thành Occupied
                room.Status = "Occupied";
            }

            _unitOfWork.RoomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}