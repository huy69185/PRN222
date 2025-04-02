using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Hubs;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager")]
    public class DetailsModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IRoomService roomService,
            IBookingService bookingService,
            IHubContext<BookingHub> hubContext,
            ILogger<DetailsModel> logger)
        {
            _roomService = roomService;
            _bookingService = bookingService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public Room Room { get; set; } = new();
        public List<Booking> BookingList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid room ID.";
                return RedirectToPage("/Manager/Rooms/Index");
            }

            Room = await _roomService.GetRoomByIdAsync(id.Value);
            if (Room == null)
            {
                TempData["ErrorMessage"] = "Room not found.";
                return RedirectToPage("/Manager/Rooms/Index");
            }

            BookingList = await _bookingService.GetBookingsByRoomIdAsync(id.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int bookingId, string bookingStatus)
        {
            var managerUserCode = User.Identity?.Name;
            var success = await _bookingService.UpdateBookingStatusRazorAsync(bookingId, bookingStatus, managerUserCode);
            if (!success)
            {
                TempData["ErrorMessage"] = "Update status failed!";
            }
            else
            {
                _logger.LogInformation("Sending BookingUpdated event for booking {0}, status: {1}", bookingId, bookingStatus);
                await _hubContext.Clients.All.SendAsync("BookingUpdated", new { bookingId = bookingId });
            }

            return RedirectToPage(new { id = Room.RoomId });
        }
    }
}
