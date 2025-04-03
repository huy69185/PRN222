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

        public async Task<IActionResult> OnGetAsync(int? id, int pageNumber = 1, string sortOrder = "asc")
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
            if (Room.Campus == null)
            {
                _logger.LogWarning("Campus is null for RoomId: {0}", id.Value);
            }
            else
            {
                _logger.LogInformation("CampusName: {0}", Room.Campus.CampusName);
            }
            // Lấy danh sách booking
            var allBookings = await _bookingService.GetBookingsByRoomIdAsync(id.Value);

            // Sắp xếp theo StartTime dựa trên sortOrder
            allBookings = sortOrder == "desc"
                ? allBookings.OrderByDescending(b => b.StartTime).ToList()
                : allBookings.OrderBy(b => b.StartTime).ToList();

            const int pageSize = 4;
            var totalBookings = allBookings.Count;
            var totalPages = (int)Math.Ceiling(totalBookings / (double)pageSize);
            BookingList = allBookings.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Lưu thông tin vào ViewData để sử dụng trong view
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;
            ViewData["SortOrder"] = sortOrder;

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int bookingId, string bookingStatus, int pageNumber = 1)
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

            return RedirectToPage(new { id = Room.RoomId, pageNumber = pageNumber });
        }
    }
}