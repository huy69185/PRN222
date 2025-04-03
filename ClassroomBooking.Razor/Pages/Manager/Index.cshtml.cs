using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Hubs;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ClassroomBooking.Presentation.Pages.Manager
{
    [Authorize(Roles = "Manager")]
    public class IndexModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IUsersService _usersService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IBookingService bookingService,
            IUsersService usersService,
            IUnitOfWork unitOfWork,
            IHubContext<BookingHub> hubContext,
            ILogger<IndexModel> logger)
        {
            _bookingService = bookingService;
            _usersService = usersService;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger;
        }

        public List<Booking> BookingList { get; set; } = new();
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

            var allBookings = await _bookingService.GetAllBookingsAsync();

            var filteredBookings = allBookings
                .Where(b => b.RoomSlots.Any(rs => rs.Room != null && rs.Room.CampusId == manager.CampusId))
                .ToList();

            TotalPages = (int)Math.Ceiling(filteredBookings.Count / (double)PageSize);

            BookingList = filteredBookings
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int bookingId, string bookingStatus)
        {
            var managerUserCode = User.Identity.Name;
            if (string.IsNullOrEmpty(managerUserCode))
            {
                ModelState.AddModelError("", "Manager user code not found. Please log in again.");
                return RedirectToPage();
            }

            var manager = await _usersService.GetUserAsync(managerUserCode);
            if (manager == null)
            {
                ModelState.AddModelError("", "Manager not found.");
                return RedirectToPage();
            }

            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                ModelState.AddModelError("", "Booking not found.");
                return RedirectToPage();
            }

            var isBookingInManagerCampus = booking.RoomSlots.Any(rs => rs.Room != null && rs.Room.CampusId == manager.CampusId);
            if (!isBookingInManagerCampus)
            {
                ModelState.AddModelError("", "You can only manage bookings in your campus.");
                return RedirectToPage();
            }

            var success = await _bookingService.UpdateBookingStatusRazorAsync(bookingId, bookingStatus, managerUserCode);
            if (!success)
            {
                ModelState.AddModelError("", "Update status failed!");
            }
            else
            {
                // Gửi thông báo qua SignalR
                _logger.LogInformation("Sending BookingUpdated event for booking {0}, status: {1}", bookingId, bookingStatus);
                await _hubContext.Clients.All.SendAsync("BookingUpdated", new { bookingId = bookingId });
            }

            return RedirectToPage(new { pageNumber = CurrentPage });
        }
    }
}