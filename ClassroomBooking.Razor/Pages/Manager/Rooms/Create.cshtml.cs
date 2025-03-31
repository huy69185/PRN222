using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms
{
    [Authorize(Roles = "Manager")]
    public class CreateModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly IUsersService _usersService;

        public CreateModel(IRoomService roomService, ICampusService campusService, IUsersService usersService)
        {
            _roomService = roomService;
            _campusService = campusService;
            _usersService = usersService;
        }

        [BindProperty]
        public Room Room { get; set; } = new();

        public List<SelectListItem> CampusItems { get; set; } = new();
        public List<SelectListItem> StatusItems { get; set; } = new()
        {
            new SelectListItem { Value = "Available", Text = "Available" },
            new SelectListItem { Value = "Occupied", Text = "Occupied" },
            new SelectListItem { Value = "Maintenance", Text = "Maintenance" }
        };

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Lấy CampusId của Manager từ Claims
            var managerUserCode = User.Identity.Name;
            if (string.IsNullOrEmpty(managerUserCode))
            {
                ErrorMessage = "Manager user code not found. Please log in again.";
                return;
            }

            var manager = await _usersService.GetUserAsync(managerUserCode);
            if (manager == null)
            {
                ErrorMessage = "Manager not found.";
                return;
            }

            // Chỉ hiển thị campus của Manager trong dropdown
            var managerCampus = await _campusService.GetAllCampusesAsync();
            CampusItems = managerCampus
                .Where(c => c.CampusId == manager.CampusId)
                .Select(c => new SelectListItem
                {
                    Value = c.CampusId.ToString(),
                    Text = c.CampusName
                }).ToList();

            // Đặt giá trị mặc định cho Room.CampusId
            Room.CampusId = manager.CampusId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"Room.CampusId: {Room.CampusId}");

            ModelState.Remove("Room.Campus");
            ModelState.Remove("Room.RoomSlots");

            // Lấy CampusId của Manager từ Claims
            var managerUserCode = User.Identity.Name;
            if (string.IsNullOrEmpty(managerUserCode))
            {
                ErrorMessage = "Manager user code not found. Please log in again.";
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CampusId.ToString(),
                        Text = c.CampusName
                    }).ToList();
                return Page();
            }

            var manager = await _usersService.GetUserAsync(managerUserCode);
            if (manager == null)
            {
                ErrorMessage = "Manager not found.";
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CampusId.ToString(),
                        Text = c.CampusName
                    }).ToList();
                return Page();
            }

            // Kiểm tra CampusId của phòng có khớp với CampusId của Manager không
            if (Room.CampusId != manager.CampusId)
            {
                ErrorMessage = "You can only create rooms in your campus.";
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses
                    .Where(c => c.CampusId == manager.CampusId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CampusId.ToString(),
                        Text = c.CampusName
                    }).ToList();
                return Page();
            }

            if (Room.CampusId <= 0)
            {
                ErrorMessage = "Please select a valid campus.";
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses
                    .Where(c => c.CampusId == manager.CampusId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CampusId.ToString(),
                        Text = c.CampusName
                    }).ToList();
                return Page();
            }

            // Kiểm tra trạng thái có nằm trong danh sách hợp lệ không
            if (!new[] { "Available", "Occupied", "Maintenance" }.Contains(Room.Status))
            {
                ErrorMessage = "Please select a valid status.";
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses
                    .Where(c => c.CampusId == manager.CampusId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CampusId.ToString(),
                        Text = c.CampusName
                    }).ToList();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ErrorMessage = "Validation failed: " + string.Join(", ", errors);
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses
                    .Where(c => c.CampusId == manager.CampusId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CampusId.ToString(),
                        Text = c.CampusName
                    }).ToList();
                return Page();
            }

            try
            {
                await _roomService.CreateRoomAsync(Room);
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error creating room: " + ex.Message;
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses
                    .Where(c => c.CampusId == manager.CampusId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CampusId.ToString(),
                        Text = c.CampusName
                    }).ToList();
                return Page();
            }
        }
    }
}