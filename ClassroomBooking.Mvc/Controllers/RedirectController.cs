using Microsoft.AspNetCore.Mvc;

namespace ClassroomBooking.Mvc.Controllers
{
    public class RedirectController : Controller
    {
        [Route("redirect-to-login")]
        public IActionResult RedirectToLogin()
        {
            return Redirect("http://localhost:5001/Account/Login");
        }

        [Route("redirect-to-logout")]
        public IActionResult RedirectToLogout()
        {
            return Redirect("http://localhost:5001/Account/Logout");
        }

        [Route("redirect-to-access-denied")]
        public IActionResult RedirectToAccessDenied()
        {
            return Redirect("http://localhost:5001/Account/AccessDenied");
        }
    }
}