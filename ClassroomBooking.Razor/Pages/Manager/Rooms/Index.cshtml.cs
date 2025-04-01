using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms
{
    [Authorize(Roles = "Manager")]
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IUsersService _usersService;
        private readonly IBookingService _bookingService;

        public IndexModel(IRoomService roomService, IUsersService usersService, IBookingService bookingService)
        {
            _roomService = roomService;
            _usersService = usersService;
            _bookingService = bookingService;
        }

        public List<Room> Rooms { get; set; } = new();
        public Dictionary<int, int> CapacityLeft { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 5;

        public async Task<IActionResult> OnGetAsync(int? pageNumber)
        {
            CurrentPage = pageNumber ?? 1;

            var managerUserCode = User.Identity.Name;
            if (string.IsNullOrEmpty(managerUserCode))
            {
                ModelState.AddModelError("", "Manager user code not found. Please log in again.");
                return Page();
            }

            var manager = await _usersService.GetUserAsync(managerUserCode);
            if (manager == null)
            {
                ModelState.AddModelError("", "Manager not found.");
                return Page();
            }

            var allRooms = await _roomService.GetAllRoomsAsync();
            var filteredRooms = allRooms
                .Where(r => r.CampusId == manager.CampusId)
                .ToList();

            TotalPages = (int)Math.Ceiling(filteredRooms.Count / (double)PageSize);
            Rooms = filteredRooms
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            foreach (var room in Rooms)
            {
                var bookings = await _bookingService.GetBookingsByRoomIdAsync(room.RoomId);
                var currentTime = DateTime.Now;
                var activeBookings = bookings
                    .Where(b => b.StartTime <= currentTime && b.EndTime >= currentTime
                             && b.BookingStatus != "Denied" && b.BookingStatus != "Cancelled")
                    .ToList();

                int seatsBooked = activeBookings
                    .Sum(b => b.RoomSlots
                        .Where(rs => rs.RoomId == room.RoomId)
                        .Sum(rs => rs.SeatsBooked));

                int capacityLeft = room.Capacity - seatsBooked;
                CapacityLeft[room.RoomId] = capacityLeft < 0 ? 0 : capacityLeft;
            }

            return Page();
        }
    }
}