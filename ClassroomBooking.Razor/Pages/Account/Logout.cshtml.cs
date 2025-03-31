using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Interfaces;

namespace ClassroomBooking.Presentation.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IUsersService _usersService;
        private readonly IUnitOfWork _unitOfWork;
        public LogoutModel(IUsersService usersService, IUnitOfWork unitOfWork)
        {
            _usersService = usersService;
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> OnPostAsync()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("SessionToken");
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return RedirectToPage("/Account/Login");
        }
    }
}