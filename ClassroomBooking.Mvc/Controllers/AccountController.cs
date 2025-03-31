using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ClassroomBooking.Mvc.Controllers
{
    public class AccountController : Controller
    {
        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Xóa cookie xác thực
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa cookie SessionToken
            Response.Cookies.Delete("SessionToken");

            // Thêm header chống cache
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Chuyển hướng về trang Login
            return Redirect("http://localhost:5001/Account/Login");
        }
    }
}