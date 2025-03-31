using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClassroomBooking.Mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;

        public RoomsController(IRoomService roomService, IBookingService bookingService)
        {
            _roomService = roomService;
            _bookingService = bookingService;
        }

        // GET: /api/rooms?campusId=123 => trả về danh sách room
        [HttpGet]
        public async Task<IActionResult> GetRoomsByCampus(int campusId)
        {
            var rooms = await _roomService.GetRoomsByCampusAsync(campusId);
            return Ok(rooms);
        }

        // GET: /api/rooms/getRoomCapacityLeft?roomId=1&startTime=2025-09-01T14:00&endTime=2025-09-01T16:00
        [HttpGet("getRoomCapacityLeft")]
        public async Task<IActionResult> GetRoomCapacityLeft(int roomId, DateTime startTime, DateTime endTime)
        {
            int capacityLeft = await _bookingService.CalculateCapacityLeftAsync(roomId, startTime, endTime);
            return Ok(capacityLeft);
        }
    }
}