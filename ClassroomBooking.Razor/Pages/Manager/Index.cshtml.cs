using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClassroomBooking.Presentation.Pages.Manager
{
    [Authorize(Roles = "Manager")]
    public class IndexModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IUsersService _usersService;
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IBookingService bookingService, IUsersService usersService, IUnitOfWork unitOfWork)
        {
            _bookingService = bookingService;
            _usersService = usersService;
            _unitOfWork = unitOfWork;
        }

        public List<Booking> BookingList { get; set; } = new();
        public int CurrentPage { get; set; } = 1; // Default to page 1
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 5; // 5 records per page

        public async Task<IActionResult> OnGetAsync(int? pageNumber)
        {
            // Set the current page (default to 1 if not provided)
            CurrentPage = pageNumber ?? 1;

            // Lấy UserCode của Manager từ Claims
            var managerUserCode = User.Identity.Name;
            if (string.IsNullOrEmpty(managerUserCode))
            {
                ModelState.AddModelError("", "Manager user code not found. Please log in again.");
                return Page();
            }

            // Lấy thông tin Manager, bao gồm CampusId
            var manager = await _usersService.GetUserAsync(managerUserCode);
            if (manager == null)
            {
                ModelState.AddModelError("", "Manager not found.");
                return Page();
            }

            // Lấy tất cả bookings
            var allBookings = await _bookingService.GetAllBookingsAsync();

            // Lọc bookings theo CampusId của Manager
            var filteredBookings = allBookings
                .Where(b => b.RoomSlots.Any(rs => rs.Room != null && rs.Room.CampusId == manager.CampusId))
                .ToList();

            // Tính tổng số trang
            TotalPages = (int)Math.Ceiling(filteredBookings.Count / (double)PageSize);

            // Lấy bookings cho trang hiện tại
            BookingList = filteredBookings
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int bookingId, string bookingStatus)
        {
            // Lấy UserCode của Manager từ Claims
            var managerUserCode = User.Identity.Name;
            if (string.IsNullOrEmpty(managerUserCode))
            {
                ModelState.AddModelError("", "Manager user code not found. Please log in again.");
                return RedirectToPage();
            }

            // Lấy thông tin Manager, bao gồm CampusId
            var manager = await _usersService.GetUserAsync(managerUserCode);
            if (manager == null)
            {
                ModelState.AddModelError("", "Manager not found.");
                return RedirectToPage();
            }

            // Lấy booking để kiểm tra CampusId
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                ModelState.AddModelError("", "Booking not found.");
                return RedirectToPage();
            }

            // Kiểm tra xem booking có thuộc campus của Manager không
            var isBookingInManagerCampus = booking.RoomSlots.Any(rs => rs.Room != null && rs.Room.CampusId == manager.CampusId);
            if (!isBookingInManagerCampus)
            {
                ModelState.AddModelError("", "You can only manage bookings in your campus.");
                return RedirectToPage();
            }

            // Cập nhật trạng thái booking
            var success = await _bookingService.UpdateBookingStatusRazorAsync(bookingId, bookingStatus, managerUserCode);
            if (!success)
            {
                ModelState.AddModelError("", "Update status failed!");
            }

            return RedirectToPage(new { pageNumber = CurrentPage }); // Redirect to the current page after updating status
        }
    }
}