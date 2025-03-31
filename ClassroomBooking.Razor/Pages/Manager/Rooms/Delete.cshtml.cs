using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms // Thay Admin thành Manager
{
    [Authorize(Roles = "Manager")] // Thay Admin thành Manager
    public class DeleteModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;

        public DeleteModel(IRoomService roomService, IBookingService bookingService)
        {
            _roomService = roomService;
            _bookingService = bookingService;
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
                // L?y t?t c? các Booking liên quan ??n Room
                var bookings = await _bookingService.GetBookingsByRoomIdAsync(id.Value);

                // Th?i gian Delete (hi?n t?i)
                var deleteTime = DateTime.Now;

                // L?y UserCode c?a Manager t? Claims
                var managerUserCode = User.Identity.Name;
                if (string.IsNullOrEmpty(managerUserCode))
                {
                    throw new Exception("Manager user code not found. Please log in again.");
                }

                // Chuy?n tr?ng thái các Booking có StartTime sau th?i gian Delete thành "Cancelled"
                foreach (var booking in bookings)
                {
                    if (booking.StartTime > deleteTime)
                    {
                        await _bookingService.UpdateBookingStatusAsync(booking.BookingId, "Cancelled", managerUserCode);
                    }
                }

                // Chuy?n tr?ng thái Room thành Maintenance
                await _roomService.DeleteRoomAsync(id.Value);

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error setting room to Maintenance: " + ex.Message;
                return Page();
            }
        }
    }
}