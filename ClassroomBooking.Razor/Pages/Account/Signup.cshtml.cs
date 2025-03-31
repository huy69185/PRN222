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
        private readonly ICampusService _campusService;

        public SignupModel(IUsersService usersService, IDepartmentService departmentService, ICampusService campusService)
        {
            _usersService = usersService;
            _departmentService = departmentService;
            _campusService = campusService;
        }

        [BindProperty]
        public Users User { get; set; } = new();

        [BindProperty]
        public string SelectedCampusName { get; set; } = string.Empty; // Thuộc tính để lưu CampusName được chọn từ dropdown

        public List<SelectListItem> CampusItems { get; set; } = new();
        public List<SelectListItem> DepartmentItems { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            await LoadDropdowns();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"SelectedCampusName: {SelectedCampusName}, User.DepartmentId: {User.DepartmentId}");

            ModelState.Remove("User.Department");
            ModelState.Remove("User.Campus");
            ModelState.Remove("User.Bookings");

            // Kiểm tra CampusName được chọn
            if (string.IsNullOrEmpty(SelectedCampusName))
            {
                ErrorMessage = "Please select a valid campus.";
                await LoadDropdowns();
                return Page();
            }

            // Ánh xạ CampusName sang CampusId
            var campuses = await _campusService.GetAllCampusesAsync();
            var selectedCampus = campuses.FirstOrDefault(c => c.CampusName == SelectedCampusName);
            if (selectedCampus == null)
            {
                ErrorMessage = "Invalid campus selected.";
                await LoadDropdowns();
                return Page();
            }
            User.CampusId = selectedCampus.CampusId;

            if (User.DepartmentId <= 0)
            {
                ErrorMessage = "Please select a valid department.";
                await LoadDropdowns();
                return Page();
            }

            // Kiểm tra Department có thuộc Campus đã chọn không
            var department = await _departmentService.GetDepartmentByIdAsync(User.DepartmentId);
            if (department == null || department.CampusId != User.CampusId)
            {
                ErrorMessage = "The selected department does not belong to the chosen campus.";
                await LoadDropdowns();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ErrorMessage = "Validation failed: " + string.Join(", ", errors);
                await LoadDropdowns();
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
                await LoadDropdowns();
                return Page();
            }
        }

        private async Task LoadDropdowns()
        {
            var campuses = await _campusService.GetAllCampusesAsync();
            CampusItems = campuses.Select(c => new SelectListItem
            {
                Value = c.CampusName, // Sử dụng CampusName làm giá trị của dropdown
                Text = c.CampusName
            }).ToList();

            var departments = await _departmentService.GetAllDepartmentsAsync();
            DepartmentItems = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.DepartmentName
            }).ToList();
        }
    }
}