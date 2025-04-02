using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Hubs;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms
{
    [Authorize(Roles = "Manager")]
    public class EditModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<BookingHub> _hubContext;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IRoomService roomService,
            ICampusService campusService,
            Microsoft.AspNetCore.SignalR.IHubContext<BookingHub> hubContext,
            ILogger<EditModel> logger)
        {
            _roomService = roomService;
            _campusService = campusService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [BindProperty]
        public Room Room { get; set; } = new();

        // Các thuộc tính để nhận giá trị cập nhật capacity nếu chuyển từ Occupied sang Available
        [BindProperty]
        public bool ResetCapacity { get; set; } = false;
        [BindProperty]
        public int AdditionalSeats { get; set; } = 0;

        // Giả sử: nếu room đang Occupied, availableSeats = 0; nếu không thì availableSeats = Room.Capacity.
        public int AvailableSeats { get; set; }

        public List<SelectListItem> CampusItems { get; set; } = new();
        public List<SelectListItem> StatusItems { get; set; } = new()
        {
            new SelectListItem { Value = "Available", Text = "Available" },
            new SelectListItem { Value = "Occupied", Text = "Occupied" },
            new SelectListItem { Value = "Maintenance", Text = "Maintenance" }
        };

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Room = await _roomService.GetRoomByIdAsync(id.Value);
            if (Room == null)
                return NotFound();

            // Giả sử: nếu Room.Status = "Occupied" thì availableSeats = 0, ngược lại lấy Room.Capacity
            AvailableSeats = Room.Status == "Occupied" ? 0 : Room.Capacity;

            var campuses = await _campusService.GetAllCampusesAsync();
            CampusItems = campuses.Select(c => new SelectListItem
            {
                Value = c.CampusId.ToString(),
                Text = c.CampusName,
                Selected = c.CampusId == Room.CampusId
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Loại bỏ các ModelState không cần thiết
            ModelState.Remove("Room.Campus");
            ModelState.Remove("Room.RoomSlots");

            if (Room.CampusId <= 0)
            {
                ErrorMessage = "Please select a valid campus.";
                await LoadCampusItemsAsync();
                return Page();
            }

            if (!new[] { "Available", "Occupied", "Maintenance" }.Contains(Room.Status))
            {
                ErrorMessage = "Please select a valid status.";
                await LoadCampusItemsAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                ErrorMessage = "Validation failed: " + string.Join(", ", errors);
                await LoadCampusItemsAsync();
                return Page();
            }

            try
            {
                await _roomService.UpdateRoomAsync(Room, ResetCapacity, AdditionalSeats);

                // Gửi sự kiện RoomUpdated qua SignalR
                _logger.LogInformation("Sending RoomUpdated event for room {0}", Room.RoomId);
                await _hubContext.Clients.All.SendAsync("RoomUpdated", new { roomId = Room.RoomId });

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error updating room: " + ex.Message;
                await LoadCampusItemsAsync();
                return Page();
            }
        }

        private async Task LoadCampusItemsAsync()
        {
            var campuses = await _campusService.GetAllCampusesAsync();
            CampusItems = campuses.Select(c => new SelectListItem
            {
                Value = c.CampusId.ToString(),
                Text = c.CampusName
            }).ToList();
        }
    }
}
