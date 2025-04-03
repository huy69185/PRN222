using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Hubs;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager")]
    public class CreateModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly IUsersService _usersService;
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IRoomService roomService,
            ICampusService campusService,
            IUsersService usersService,
            IHubContext<BookingHub> hubContext,
            ILogger<CreateModel> logger)
        {
            _roomService = roomService;
            _campusService = campusService;
            _usersService = usersService;
            _hubContext = hubContext;
            _logger = logger;
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
            // Lấy thông tin Manager từ Claims hoặc service
            var managerUserCode = User.Identity?.Name;
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
            var managerCampuses = await _campusService.GetAllCampusesAsync();
            CampusItems = managerCampuses
                .Where(c => c.CampusId == manager.CampusId)
                .Select(c => new SelectListItem
                {
                    Value = c.CampusId.ToString(),
                    Text = c.CampusName
                }).ToList();

            // Đặt mặc định cho Room.CampusId theo Manager
            Room.CampusId = manager.CampusId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"Room.CampusId: {Room.CampusId}");

            // Loại bỏ các ModelState không cần thiết nếu có
            ModelState.Remove("Room.Campus");
            ModelState.Remove("Room.RoomSlots");

            // Lấy thông tin Manager
            var managerUserCode = User.Identity?.Name;
            if (string.IsNullOrEmpty(managerUserCode))
            {
                ErrorMessage = "Manager user code not found. Please log in again.";
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses.Select(c => new SelectListItem
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
                CampusItems = campuses.Select(c => new SelectListItem
                {
                    Value = c.CampusId.ToString(),
                    Text = c.CampusName
                }).ToList();
                return Page();
            }

            // Kiểm tra CampusId của room có khớp với của Manager không
            if (Room.CampusId != manager.CampusId)
            {
                ErrorMessage = "You can only create rooms in your campus.";
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses.Where(c => c.CampusId == manager.CampusId)
                    .Select(c => new SelectListItem { Value = c.CampusId.ToString(), Text = c.CampusName }).ToList();
                return Page();
            }

            if (Room.CampusId <= 0)
            {
                ErrorMessage = "Please select a valid campus.";
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses.Where(c => c.CampusId == manager.CampusId)
                    .Select(c => new SelectListItem { Value = c.CampusId.ToString(), Text = c.CampusName }).ToList();
                return Page();
            }

            if (!new[] { "Available", "Occupied", "Maintenance" }.Contains(Room.Status))
            {
                ErrorMessage = "Please select a valid status.";
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses.Where(c => c.CampusId == manager.CampusId)
                    .Select(c => new SelectListItem { Value = c.CampusId.ToString(), Text = c.CampusName }).ToList();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                ErrorMessage = "Validation failed: " + string.Join(", ", errors);
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses.Where(c => c.CampusId == manager.CampusId)
                    .Select(c => new SelectListItem { Value = c.CampusId.ToString(), Text = c.CampusName }).ToList();
                return Page();
            }

            try
            {
                await _roomService.CreateRoomAsync(Room);

                // Sau khi tạo thành công, Room.RoomId sẽ được tự động cập nhật
                _logger.LogInformation("Room created with ID {0}", Room.RoomId);

                // Gửi sự kiện RoomCreated qua SignalR; nếu có lỗi SignalR, log lỗi nhưng không ảnh hưởng đến chức năng
                try
                {
                    _logger.LogInformation("Sending RoomCreated event for room {0}", Room.RoomId);
                    await _hubContext.Clients.All.SendAsync("RoomCreated", new { roomId = Room.RoomId });
                    return RedirectToPage("/Manager/Rooms/Index");

                }
                catch (Exception exSignalR)
                {
                    _logger.LogError("SignalR error when sending RoomCreated: {0}", exSignalR.Message);
                    return Page();

                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating room: {0}", ex.Message);
                ErrorMessage = "Error creating room: " + ex.Message;
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses.Where(c => c.CampusId == manager.CampusId)
                    .Select(c => new SelectListItem { Value = c.CampusId.ToString(), Text = c.CampusName }).ToList();
                return Page();
            }
        }
    }
}
