using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms // Thay Admin thành Manager
{
    [Authorize(Roles = "Manager")] // Thay Admin thành Manager
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;

        public IndexModel(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public List<Room> Rooms { get; set; } = new();

        public async Task OnGetAsync()
        {
            Rooms = await _roomService.GetAllRoomsAsync();
        }
    }
}