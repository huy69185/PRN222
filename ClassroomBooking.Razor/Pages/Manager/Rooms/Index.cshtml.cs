using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms
{
    [Authorize(Roles = "Manager")]
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IUsersService _usersService;

        public IndexModel(IRoomService roomService, IUsersService usersService)
        {
            _roomService = roomService;
            _usersService = usersService;
        }

        public List<Room> Rooms { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy UserCode của Manager từ Claims
            var managerUserCode = User.Identity.Name;
            if (string.IsNullOrEmpty(managerUserCode))
            {
                ModelState.AddModelError("", "Manager user code not found. Please log in again.");
                return Page();
            }

            // Lấy thông tin Manager, bao gồm CampusId
            var manager = await _usersService.GetUserAsync(managerUserCode);
            if (manager == null)
            {
                ModelState.AddModelError("", "Manager not found.");
                return Page();
            }

            // Lấy tất cả phòng
            var allRooms = await _roomService.GetAllRoomsAsync();

            // Lọc phòng theo CampusId của Manager
            Rooms = allRooms
                .Where(r => r.CampusId == manager.CampusId)
                .ToList();

            return Page();
        }
    }
}