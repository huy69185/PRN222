﻿using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Dtos;
using ClassroomBooking.Service.Hubs;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ClassroomBooking.Service
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoomService _roomService;
        private readonly IHubContext<BookingHub> _hubContext;

        public BookingService(IUnitOfWork unitOfWork, IRoomService roomService, IHubContext<BookingHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _roomService = roomService;
            _hubContext = hubContext;
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _unitOfWork.BookingRepository.GetAllAsync(includeProperties: "User,RoomSlots,RoomSlots.Room");
        }

        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            return await _unitOfWork.BookingRepository.GetFirstOrDefaultAsync(
                b => b.BookingId == bookingId, "User,RoomSlots,RoomSlots.Room");
        }

        public async Task<List<Booking>> GetBookingsByUserCodeAsync(string userCode)
        {
            var user = await _unitOfWork.UsersRepository.GetFirstOrDefaultAsync(u => u.UserCode == userCode);
            if (user == null)
                return new List<Booking>();

            return await _unitOfWork.BookingRepository.GetAllAsync(
                b => b.StudentId == user.USerId, "User,RoomSlots,RoomSlots.Room");
        }

        public async Task<List<BookingDto>> GetBookingsByUserCodeWithFilterAsync(
            string userCode,
            string searchRoom,
            string searchPurpose,
            string statusFilter)
        {
            var user = await _unitOfWork.UsersRepository.GetFirstOrDefaultAsync(u => u.UserCode == userCode);
            if (user == null)
                return new List<BookingDto>();

            var all = await _unitOfWork.BookingRepository.GetAllAsync(
                b => b.StudentId == user.USerId,
                "RoomSlots,RoomSlots.Room");

            if (!string.IsNullOrEmpty(searchRoom))
            {
                all = all.Where(b =>
                    b.RoomSlots.Any(rs => rs.Room != null &&
                        rs.Room.RoomName.ToLower().Contains(searchRoom.ToLower()))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchPurpose))
            {
                all = all.Where(b => b.Purpose.ToLower().Contains(searchPurpose.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                all = all.Where(b => b.BookingStatus.Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var result = all.Select(b => MapToDto(b)).ToList();
            return result;
        }

        public async Task UpdateBookingStatusAsync(int bookingId, string status, string managerUserCode)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new Exception("Booking not found.");

            var student = await _unitOfWork.UsersRepository.GetByIdAsync(booking.StudentId);
            var manager = await _unitOfWork.UsersRepository.GetFirstOrDefaultAsync(
                u => u.UserCode == managerUserCode && u.Role == "Manager");

            if (manager == null || manager.DepartmentId != student.DepartmentId)
                throw new Exception("Only the Manager of the student's department can approve this booking.");

            booking.BookingStatus = status;
            _unitOfWork.BookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync();

            var roomSlot = booking.RoomSlots.FirstOrDefault();
            if (roomSlot != null)
            {
                int capacityLeft = await CalculateCapacityLeftAsync(roomSlot.RoomId, booking.StartTime, booking.EndTime);
                await _roomService.UpdateRoomStatusBasedOnCapacityAsync(roomSlot.RoomId, capacityLeft);
            }

            await _hubContext.Clients.All.SendAsync("ReceiveBookingNotification", $"Booking {bookingId} updated to {status}");
        }

        public async Task<bool> UpdateBookingStatusRazorAsync(int bookingId, string status, string managerUserCode)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return false;

            var student = await _unitOfWork.UsersRepository.GetByIdAsync(booking.StudentId);
            var manager = await _unitOfWork.UsersRepository.GetFirstOrDefaultAsync(u => u.UserCode == managerUserCode && u.Role == "Manager");

            if (manager == null || manager.DepartmentId != student.DepartmentId)
                throw new Exception("Only the Manager of the student's department can approve this booking.");

            booking.BookingStatus = status;
            _unitOfWork.BookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync();

            var roomSlot = booking.RoomSlots.FirstOrDefault();
            if (roomSlot != null)
            {
                int capacityLeft = await CalculateCapacityLeftAsync(roomSlot.RoomId, booking.StartTime, booking.EndTime);
                await _roomService.UpdateRoomStatusBasedOnCapacityAsync(roomSlot.RoomId, capacityLeft);
            }

            await _hubContext.Clients.All.SendAsync("ReceiveBookingNotification", $"Booking {bookingId} updated to {status}");
            return true;
        }

        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return false;

            var roomSlots = await _unitOfWork.RoomSlotRepository.GetAllAsync(rs => rs.BookingId == bookingId);
            foreach (var roomSlot in roomSlots)
            {
                await _unitOfWork.RoomSlotRepository.DeleteAsync(roomSlot);
            }

            await _unitOfWork.BookingRepository.DeleteAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            if (roomSlots.Any())
            {
                var roomId = roomSlots.First().RoomId;
                int capacityLeft = await CalculateCapacityLeftAsync(roomId, booking.StartTime, booking.EndTime);
                await _roomService.UpdateRoomStatusBasedOnCapacityAsync(roomId, capacityLeft);
            }

            return true;
        }

        public async Task<int> CalculateCapacityLeftAsync(int roomId, DateTime start, DateTime end)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room == null)
                return 0;

            var bookings = await GetAllBookingsAsync();
            var overlapBookings = bookings.Where(b =>
                b.StartTime < end &&
                b.EndTime > start &&
                b.BookingStatus != "Denied" &&
                b.BookingStatus != "Cancelled").ToList();

            int sumSeats = overlapBookings.Sum(b => b.RoomSlots.Where(rs => rs.RoomId == roomId)
                                            .Sum(rs => rs.SeatsBooked));
            int left = room.Capacity - sumSeats;
            await _roomService.UpdateRoomStatusBasedOnCapacityAsync(roomId, left);
            return left < 0 ? 0 : left;
        }

        public async Task CreateBookingWithRoomSlotAsync(BookingDto dto, int roomId, int seatsWanted)
        {
            if (dto.EndTime <= dto.StartTime)
                throw new Exception("EndTime must be after StartTime!");

            // Kiểm tra số slot còn lại trước khi tạo booking
            int capacityLeft = await CalculateCapacityLeftAsync(roomId, dto.StartTime, dto.EndTime);
            if (capacityLeft < seatsWanted)
                throw new Exception("Not enough seats available in the selected room.");

            // Map từ DTO sang entity Booking
            var bookingEntity = new Booking
            {
                StudentId = dto.StudentId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Purpose = dto.Purpose,
                BookingStatus = dto.BookingStatus,
                CreatedDate = DateTime.Now
            };

            bookingEntity.RoomSlots = new List<RoomSlot>
            {
                new RoomSlot
                {
                    RoomId = roomId,
                    SeatsBooked = seatsWanted,
                    SlotDate = dto.StartTime.Date
                }
            };

            await _unitOfWork.BookingRepository.AddAsync(bookingEntity);
            await _unitOfWork.SaveChangesAsync();

            // Cập nhật trạng thái phòng
            capacityLeft = await CalculateCapacityLeftAsync(roomId, dto.StartTime, dto.EndTime);
            await _roomService.UpdateRoomStatusBasedOnCapacityAsync(roomId, capacityLeft);

            await _hubContext.Clients.All.SendAsync("ReceiveBookingNotification", $"New booking created: {bookingEntity.BookingId}");
        }

        public async Task<List<Booking>> GetBookingsByRoomIdAsync(int roomId)
        {
            return await _unitOfWork.BookingRepository.GetAllAsync(
                b => b.RoomSlots.Any(rs => rs.RoomId == roomId),
                "User,RoomSlots,RoomSlots.Room");
        }

        private BookingDto MapToDto(Booking b)
        {
            return new BookingDto
            {
                BookingId = b.BookingId,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Purpose = b.Purpose,
                BookingStatus = b.BookingStatus,
                RoomSlots = b.RoomSlots.Select(rs => new RoomSlotDto
                {
                    RoomSlotId = rs.RoomSlotId,
                    SeatsBooked = rs.SeatsBooked,
                    SlotDate = rs.SlotDate,
                    Room = rs.Room == null ? null : new RoomDto
                    {
                        RoomId = rs.Room.RoomId,
                        RoomName = rs.Room.RoomName
                    }
                }).ToList()
            };
        }
    }
}