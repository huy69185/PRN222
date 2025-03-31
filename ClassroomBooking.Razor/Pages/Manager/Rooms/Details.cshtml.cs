using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms // Thay Admin thành Manager
{
    [Authorize(Roles = "Manager")] // Thay Admin thành Manager
    public class DetailsModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;

        public DetailsModel(IRoomService roomService, IBookingService bookingService)
        {
            _roomService = roomService;
            _bookingService = bookingService;
        }

        public Room Room { get; set; } = new();
        public List<Booking> BookingList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid room ID.";
                return RedirectToPage("/Manager/Rooms/Index"); // Thay Admin thành Manager
            }

            Room = await _roomService.GetRoomByIdAsync(id.Value);
            if (Room == null)
            {
                TempData["ErrorMessage"] = "Room not found.";
                return RedirectToPage("/Manager/Rooms/Index"); // Thay Admin thành Manager
            }

            BookingList = await _bookingService.GetBookingsByRoomIdAsync(id.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int bookingId, string bookingStatus)
        {
            var managerUserCode = User.Identity.Name; // L?y UserCode c?a Manager t? Claims
            var success = await _bookingService.UpdateBookingStatusRazorAsync(bookingId, bookingStatus, managerUserCode);
            if (!success)
            {
                TempData["ErrorMessage"] = "Update status failed!";
                return RedirectToPage("/Manager/Rooms/Details", new { id = Room.RoomId });
            }

            return RedirectToPage("/Manager/Rooms/Details", new { id = Room.RoomId });
        }
    }
}