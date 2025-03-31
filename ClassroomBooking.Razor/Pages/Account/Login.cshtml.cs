using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ClassroomBooking.Presentation.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IUsersService _usersService;

        public LoginModel(IUsersService usersService)
        {
            _usersService = usersService;
        }

        [BindProperty]
        public LoginViewModel LoginVM { get; set; } = new LoginViewModel();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                // Xác thực user theo service của bạn
                var user = await _usersService.LoginAsync(LoginVM.UserCode, LoginVM.Password);

                var role = user.Role?.Trim();
                if (string.IsNullOrEmpty(role))
                    throw new Exception("User role is not defined!");

                // Nếu không phải Manager thì mặc định là Student
                var normalizedRole = role.Equals("Manager", StringComparison.OrdinalIgnoreCase) ? "Manager" : "Student";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserCode),
                    new Claim(ClaimTypes.Role, normalizedRole),
                    new Claim("CampusId", user.CampusId.ToString()) // Thêm CampusId vào Claims
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    // Bạn có thể cấu hình thời gian hết hạn tại đây nếu muốn:
                    // ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                };

                // Chỉ sử dụng Cookie Authentication của .NET, không tạo SessionToken thủ công
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Chuyển hướng dựa trên role
                if (normalizedRole == "Manager")
                    return Redirect("http://localhost:5001/Manager/Index");
                else
                    return Redirect("http://localhost:5000/Bookings/Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }

    public class LoginViewModel
    {
        public string UserCode { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}