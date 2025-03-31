using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClassroomBooking.Presentation.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Manager")) // Thay Admin thành Manager
                {
                    return RedirectToPage("/Manager/Index");
                }
                else if (User.IsInRole("Student"))
                {
                    return RedirectToPage("/Bookings/Booking");
                }
            }

            return Page();
        }
    }
}