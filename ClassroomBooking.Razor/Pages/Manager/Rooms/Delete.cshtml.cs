using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Hubs;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager")]
    public class DeleteModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(
            IRoomService roomService,
            IBookingService bookingService,
            IHubContext<BookingHub> hubContext,
            ILogger<DeleteModel> logger)
        {
            _roomService = roomService;
            _bookingService = bookingService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [BindProperty]
        public Room Room { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Room = await _roomService.GetRoomByIdAsync(id.Value);
            if (Room == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                // Trước khi xóa phòng, cập nhật trạng thái các booking liên quan
                var bookings = await _bookingService.GetBookingsByRoomIdAsync(id.Value);
                var deleteTime = DateTime.Now;
                var managerUserCode = User.Identity?.Name;
                if (string.IsNullOrEmpty(managerUserCode))
                    throw new Exception("Manager user code not found. Please log in again.");

                foreach (var booking in bookings)
                {
                    if (booking.StartTime > deleteTime)
                    {
                        await _bookingService.UpdateBookingStatusAsync(booking.BookingId, "Cancelled", managerUserCode);
                        _logger.LogInformation("Sending BookingCancelled event for booking {0} due to room deletion", booking.BookingId);
                        await _hubContext.Clients.All.SendAsync("BookingCancelled", new { bookingId = booking.BookingId });
                    }
                }

                await _roomService.DeleteRoomAsync(id.Value);

                _logger.LogInformation("Sending RoomDeleted event for room {0}", id);
                await _hubContext.Clients.All.SendAsync("RoomDeleted", new { roomId = id });

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error setting room to Maintenance: {0}", ex.Message);
                ErrorMessage = "Error setting room to Maintenance: " + ex.Message;
                return Page();
            }
        }
    }
}
