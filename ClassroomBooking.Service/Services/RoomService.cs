using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Hubs;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomBooking.Service
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<BookingHub> _hubContext;

        public RoomService(IUnitOfWork unitOfWork, IHubContext<BookingHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
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

            // Gửi thông báo qua SignalR sau khi tạo phòng
            await _hubContext.Clients.All.SendAsync("RoomCreated", new { roomId = room.RoomId });
        }

        public async Task UpdateRoomAsync(Room room, bool resetCapacity = false, int additionalSeats = 0)
        {
            var existingRoom = await _unitOfWork.RoomRepository.GetByIdAsync(room.RoomId);
            if (existingRoom == null) throw new Exception("Room not found.");

            // Nếu phòng chuyển từ Occupied sang Available, và available capacity = 0 (logic này do giao diện quyết định)
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
            else
            {
                // Nếu không có điều kiện đặc biệt, cập nhật các thuộc tính khác theo giá trị mới
                existingRoom.Capacity = room.Capacity;
            }

            existingRoom.RoomName = room.RoomName;
            existingRoom.CampusId = room.CampusId;
            existingRoom.Status = room.Status;
            existingRoom.Description = room.Description;

            _unitOfWork.RoomRepository.Update(existingRoom);
            await _unitOfWork.SaveChangesAsync();

            // Gửi sự kiện RoomUpdated qua SignalR
            await _hubContext.Clients.All.SendAsync("RoomUpdated", new { roomId = existingRoom.RoomId });
        }

        public async Task DeleteRoomAsync(int roomId)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room == null) throw new Exception("Room not found.");
            room.Status = "Maintenance";
            _unitOfWork.RoomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("RoomDeleted", new { roomId = roomId });
        }

        public async Task<List<Room>> GetRoomsByCampusAsync(int campusId)
        {
            return await _unitOfWork.RoomRepository.GetAllAsync(
                r => r.CampusId == campusId && r.Status != "Maintenance", "Campus");
        }
    }
}
