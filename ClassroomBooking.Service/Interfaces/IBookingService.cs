using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Dtos;


namespace ClassroomBooking.Service.Interfaces
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int bookingId);
        Task<List<Booking>> GetBookingsByUserCodeAsync(string userCode);
        Task<List<BookingDto>> GetBookingsByUserCodeWithFilterAsync(
           string userCode,
           string searchRoom,
           string searchPurpose,
           string statusFilter
       );
        Task UpdateBookingStatusAsync(int bookingId, string status, string managerUserCode);
        Task<bool> UpdateBookingStatusRazorAsync(int bookingId, string status, string managerUserCode);
        Task<bool> DeleteBookingAsync(int bookingId);
        Task<int> CalculateCapacityLeftAsync(int roomId, DateTime start, DateTime end);
        Task CreateBookingWithRoomSlotAsync(BookingDto dto, int roomId, int seatsWanted);
        Task<List<Booking>> GetBookingsByRoomIdAsync(int roomId);

    }
}
