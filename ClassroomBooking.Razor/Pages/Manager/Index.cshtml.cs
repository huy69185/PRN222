using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClassroomBooking.Presentation.Pages.Manager // Thay Admin thành Manager
{
    [Authorize(Roles = "Manager")] // Thay Admin thành Manager
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

        public async Task<IActionResult> OnGetAsync()
        {
            // Không cần check sessionToken nữa, Cookie Authentication đã lo
            BookingList = await _bookingService.GetAllBookingsAsync();
            return Page();
        }


        public async Task<IActionResult> OnPostUpdateStatusAsync(int bookingId, string bookingStatus)
        {
            var managerUserCode = User.Identity.Name; // L?y UserCode c?a Manager t? Claims
            var success = await _bookingService.UpdateBookingStatusRazorAsync(bookingId, bookingStatus, managerUserCode);
            if (!success)
            {
                ModelState.AddModelError("", "Update status failed!");
            }
            return RedirectToPage();
        }
    }
}