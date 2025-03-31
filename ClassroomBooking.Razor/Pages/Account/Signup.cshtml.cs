using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClassroomBooking.Presentation.Pages.Account
{
    public class SignupModel : PageModel
    {
        private readonly IUsersService _usersService;
        private readonly IDepartmentService _departmentService;

        public SignupModel(IUsersService usersService, IDepartmentService departmentService)
        {
            _usersService = usersService;
            _departmentService = departmentService;
        }

        [BindProperty]
        public Users User { get; set; } = new();

        public List<SelectListItem> DepartmentItems { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            DepartmentItems = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.DepartmentName
            }).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Debug giá tr? c?a User ?? ki?m tra binding
            Console.WriteLine($"User.DepartmentId: {User.DepartmentId}");

            // Xóa các l?i không liên quan trong ModelState
            ModelState.Remove("User.Department");

            // Ki?m tra tr??c khi validate ModelState
            if (User.DepartmentId <= 0)
            {
                ErrorMessage = "Please select a valid department.";
                var departments = await _departmentService.GetAllDepartmentsAsync();
                DepartmentItems = departments.Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToList();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ErrorMessage = "Validation failed: " + string.Join(", ", errors);
                var departments = await _departmentService.GetAllDepartmentsAsync();
                DepartmentItems = departments.Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToList();
                return Page();
            }

            try
            {
                await _usersService.RegisterUserAsync(User);
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error registering user: " + ex.Message;
                var departments = await _departmentService.GetAllDepartmentsAsync();
                DepartmentItems = departments.Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToList();
                return Page();
            }
        }
    }
}