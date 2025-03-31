using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms // Thay Admin th�nh Manager
{
    [Authorize(Roles = "Manager")] // Thay Admin th�nh Manager
    public class CreateModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;

        public CreateModel(IRoomService roomService, ICampusService campusService)
        {
            _roomService = roomService;
            _campusService = campusService;
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
            var campuses = await _campusService.GetAllCampusesAsync();
            CampusItems = campuses.Select(c => new SelectListItem
            {
                Value = c.CampusId.ToString(),
                Text = c.CampusName
            }).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Debug gi� tr? c?a Room ?? ki?m tra binding
            Console.WriteLine($"Room.CampusId: {Room.CampusId}");

            // X�a c�c l?i kh�ng li�n quan trong ModelState
            ModelState.Remove("Room.Campus");
            ModelState.Remove("Room.RoomSlots");

            // Ki?m tra tr??c khi validate ModelState
            if (Room.CampusId <= 0)
            {
                ErrorMessage = "Please select a valid campus.";
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusItems = campuses.Select(c => new SelectListItem
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
                CampusItems = campuses.Select(c => new SelectListItem
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
                CampusItems = campuses.Select(c => new SelectListItem
                {
                    Value = c.CampusId.ToString(),
                    Text = c.CampusName
                }).ToList();
                return Page();
            }
        }
    }
}