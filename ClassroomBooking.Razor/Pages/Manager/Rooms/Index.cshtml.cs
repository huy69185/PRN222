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
        public int CurrentPage { get; set; } = 1; // Default to page 1
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 5; // 5 rooms per page

        public async Task<IActionResult> OnGetAsync(int? pageNumber)
        {
            // Set the current page (default to 1 if not provided)
            CurrentPage = pageNumber ?? 1;

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
            var filteredRooms = allRooms
                .Where(r => r.CampusId == manager.CampusId)
                .ToList();

            // Tính tổng số trang
            TotalPages = (int)Math.Ceiling(filteredRooms.Count / (double)PageSize);

            // Lấy rooms cho trang hiện tại
            Rooms = filteredRooms
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }
    }
}