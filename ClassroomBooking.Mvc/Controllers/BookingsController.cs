using ClassroomBooking.Mvc.Models;
using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Dtos;
using ClassroomBooking.Service.Hubs;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ClassroomBooking.Mvc.Controllers
{
    [Authorize(Roles = "Student")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly IUsersService _usersService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingsController> _logger;
        private readonly IHubContext<BookingHub> _hubContext;

        public BookingsController(
            IBookingService bookingService,
            IRoomService roomService,
            ICampusService campusService,
            IUsersService usersService,
            ILogger<BookingsController> logger,
            IUnitOfWork unitOfWork,
            IHubContext<BookingHub> hubContext)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _campusService = campusService;
            _usersService = usersService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        // GET: /Bookings/Index
        public async Task<IActionResult> Index(string searchRoom = "", string searchPurpose = "", string statusFilter = "", int pageNumber = 1)
        {
            const int pageSize = 4; 

            var userCode = User.Identity?.Name;
            if (string.IsNullOrEmpty(userCode))
            {
                _logger.LogWarning("User not authenticated, redirecting to login.");
                return Redirect("http://localhost:5001/Account/Login");
            }

            _logger.LogInformation("Index called with searchRoom: {0}, searchPurpose: {1}, statusFilter: {2}, pageNumber: {3}", searchRoom, searchPurpose, statusFilter, pageNumber);

            var bookingDtos = await _bookingService.GetBookingsByUserCodeWithFilterAsync(
                userCode,
                searchRoom,
                searchPurpose,
                statusFilter
            );

            var totalBookings = bookingDtos.Count;
            var totalPages = (int)Math.Ceiling(totalBookings / (double)pageSize);

            var bookingsForPage = bookingDtos
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchRoom = searchRoom;
            ViewBag.SearchPurpose = searchPurpose;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(bookingsForPage);
        }

        // GET: /Bookings/Create
        public async Task<IActionResult> Create()
        {
            var userCode = User.Identity?.Name;
            if (string.IsNullOrEmpty(userCode))
            {
                _logger.LogWarning("User not authenticated in Create, redirecting to login.");
                return Redirect("http://localhost:5001/Account/Login");
            }

            var user = await _usersService.GetUserAsync(userCode);
            if (user == null)
            {
                _logger.LogError("User not found for userCode: {0}", userCode);
                TempData["Error"] = "User not found. Please log in again.";
                return Redirect("http://localhost:5001/Account/Login");
            }

            var model = new BookingCreateModel
            {
                StudentId = user.USerId,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1),
                CapacityLeft = 0,
                SelectedCampusId = user.CampusId
            };

            if (TempData["Purpose"] != null) model.Purpose = TempData["Purpose"].ToString();
            if (TempData["StartTime"] != null) model.StartTime = DateTime.Parse(TempData["StartTime"].ToString());
            if (TempData["EndTime"] != null) model.EndTime = DateTime.Parse(TempData["EndTime"].ToString());
            if (TempData["SeatsWanted"] != null) model.SeatsWanted = int.Parse(TempData["SeatsWanted"].ToString());
            if (TempData["SelectedRoomId"] != null) model.SelectedRoomId = int.Parse(TempData["SelectedRoomId"].ToString());
            if (TempData["CapacityLeft"] != null) model.CapacityLeft = int.Parse(TempData["CapacityLeft"].ToString());

            var campus = await _campusService.GetAllCampusesAsync();
            ViewBag.CampusList = campus.Where(c => c.CampusId == user.CampusId).ToList();

            var rooms = await _roomService.GetRoomsByCampusAsync(user.CampusId);
            ViewBag.RoomList = rooms;

            return View(model);
        }

        // POST: /Bookings/UpdateCampus
        [HttpPost]
        public IActionResult UpdateCampus(BookingCreateModel model)
        {
            TempData["Purpose"] = model.Purpose;
            TempData["StartTime"] = model.StartTime.ToString("o");
            TempData["EndTime"] = model.EndTime.ToString("o");
            TempData["SeatsWanted"] = model.SeatsWanted.ToString();
            TempData["SelectedCampusId"] = model.SelectedCampusId.ToString();
            TempData["SelectedRoomId"] = model.SelectedRoomId.ToString();
            TempData["CapacityLeft"] = model.CapacityLeft.ToString();

            return RedirectToAction("Create");
        }

        // POST: /Bookings/UpdateCapacity
        [HttpPost]
        public async Task<IActionResult> UpdateCapacity(BookingCreateModel model)
        {
            var userCode = User.Identity?.Name;
            if (string.IsNullOrEmpty(userCode))
            {
                _logger.LogWarning("User not authenticated in UpdateCapacity, redirecting to login.");
                return Redirect("http://localhost:5001/Account/Login");
            }

            var user = await _usersService.GetUserAsync(userCode);
            if (user == null)
            {
                _logger.LogError("User not found for userCode: {0}", userCode);
                TempData["Error"] = "User not found. Please log in again.";
                return Redirect("http://localhost:5001/Account/Login");
            }

            model.SelectedCampusId = user.CampusId;

            TempData["Purpose"] = model.Purpose;
            TempData["StartTime"] = model.StartTime.ToString("o");
            TempData["EndTime"] = model.EndTime.ToString("o");
            TempData["SeatsWanted"] = model.SeatsWanted.ToString();
            TempData["SelectedCampusId"] = model.SelectedCampusId.ToString();
            TempData["SelectedRoomId"] = model.SelectedRoomId.ToString();

            if (model.SelectedRoomId > 0)
            {
                var room = await _roomService.GetRoomByIdAsync(model.SelectedRoomId);
                if (room == null || room.Status != "Available" || room.CampusId != user.CampusId)
                {
                    TempData["Error"] = "Selected room is not available or does not belong to your campus.";
                    model.CapacityLeft = 0;
                }
                else
                {
                    try
                    {
                        model.CapacityLeft = await _bookingService.CalculateCapacityLeftAsync(
                            model.SelectedRoomId, model.StartTime, model.EndTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error calculating capacity: {0}", ex.Message);
                        TempData["Error"] = "Error calculating capacity: " + ex.Message;
                        model.CapacityLeft = 0;
                    }
                }
            }
            else
            {
                model.CapacityLeft = 0;
            }

            TempData["CapacityLeft"] = model.CapacityLeft.ToString();

            return RedirectToAction("Create");
        }

        // POST: /Bookings/Create
        [HttpPost]
        public async Task<IActionResult> Create(BookingCreateModel model)
        {
            var userCode = User.Identity?.Name;
            if (string.IsNullOrEmpty(userCode))
            {
                _logger.LogWarning("User not authenticated in Create POST, redirecting to login.");
                return Redirect("http://localhost:5001/Account/Login");
            }

            var user = await _usersService.GetUserAsync(userCode);
            if (user == null)
            {
                _logger.LogError("User not found for userCode: {0}", userCode);
                TempData["Error"] = "User not found. Please log in again.";
                return Redirect("http://localhost:5001/Account/Login");
            }

            model.StudentId = user.USerId;
            model.SelectedCampusId = user.CampusId;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid in Create POST.");
                TempData["Error"] = "Please fill in all required fields correctly.";
                ViewBag.CampusList = await _campusService.GetAllCampusesAsync();
                ViewBag.RoomList = await _roomService.GetRoomsByCampusAsync(user.CampusId);
                return View(model);
            }

            var room = await _roomService.GetRoomByIdAsync(model.SelectedRoomId);
            if (room == null || room.Status != "Available" || room.CampusId != user.CampusId)
            {
                TempData["Error"] = "Selected room is not available or does not belong to your campus.";
                ViewBag.CampusList = await _campusService.GetAllCampusesAsync();
                ViewBag.RoomList = await _roomService.GetRoomsByCampusAsync(user.CampusId);
                return View(model);
            }

            model.CapacityLeft = await _bookingService.CalculateCapacityLeftAsync(
                model.SelectedRoomId, model.StartTime, model.EndTime);

            if (model.SeatsWanted <= 0)
            {
                TempData["Error"] = "SeatsWanted must be greater than 0!";
                ViewBag.CampusList = await _campusService.GetAllCampusesAsync();
                ViewBag.RoomList = await _roomService.GetRoomsByCampusAsync(user.CampusId);
                return View(model);
            }

            if (model.SeatsWanted > model.CapacityLeft)
            {
                TempData["Error"] = $"Not enough seats. Capacity left = {model.CapacityLeft}.";
                ViewBag.CampusList = await _campusService.GetAllCampusesAsync();
                ViewBag.RoomList = await _roomService.GetRoomsByCampusAsync(user.CampusId);
                return View(model);
            }

            try
            {
                var dto = new BookingDto
                {
                    StudentId = model.StudentId,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Purpose = model.Purpose,
                    BookingStatus = "Pending"
                };

                var booking = await _bookingService.CreateBookingWithRoomSlotAsync(dto, model.SelectedRoomId, model.SeatsWanted);

                // Gửi thông báo qua SignalR
                _logger.LogInformation("Sending BookingCreated event for booking {0}", booking.BookingId);
                await _hubContext.Clients.All.SendAsync("BookingCreated", new { bookingId = booking.BookingId });

                _logger.LogInformation("Booking created successfully for StudentId: {0}", model.StudentId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating booking: {0}", ex.Message);
                TempData["Error"] = $"Error creating booking: {ex.Message}";
                ViewBag.CampusList = await _campusService.GetAllCampusesAsync();
                ViewBag.RoomList = await _roomService.GetRoomsByCampusAsync(user.CampusId);
                return View(model);
            }
        }

        // POST: /Bookings/Cancel
        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string searchRoom = "", string searchPurpose = "", string statusFilter = "", int pageNumber = 1)
        {
            var userCode = User.Identity?.Name;
            if (string.IsNullOrEmpty(userCode))
            {
                _logger.LogWarning("User not authenticated in Cancel, redirecting to login.");
                return Redirect("http://localhost:5001/Account/Login");
            }

            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    _logger.LogWarning("Booking with ID: {0} not found.", id);
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("Index", new { searchRoom, searchPurpose, statusFilter, pageNumber });
                }

                var user = await _usersService.GetUserAsync(userCode);
                if (booking.StudentId != user.USerId)
                {
                    _logger.LogWarning("User {0} attempted to cancel booking {1} that does not belong to them.", userCode, id);
                    TempData["Error"] = "You can only cancel your own bookings.";
                    return RedirectToAction("Index", new { searchRoom, searchPurpose, statusFilter, pageNumber });
                }

                if (booking.BookingStatus != "Pending" && booking.BookingStatus != "Approved")
                {
                    _logger.LogWarning("Booking {0} cannot be cancelled due to status: {1}", id, booking.BookingStatus);
                    TempData["Error"] = "Only Pending or Approved bookings can be cancelled.";
                    return RedirectToAction("Index", new { searchRoom, searchPurpose, statusFilter, pageNumber });
                }

                booking.BookingStatus = "Cancelled";
                _unitOfWork.BookingRepository.Update(booking);
                await _unitOfWork.SaveChangesAsync();

                // Gửi thông báo qua SignalR
                _logger.LogInformation("Sending BookingCancelled event for booking {0}", id);
                await _hubContext.Clients.All.SendAsync("BookingCancelled", new { bookingId = id });

                _logger.LogInformation("Booking {0} cancelled successfully by user {1}.", id, userCode);
                return RedirectToAction("Index", new { searchRoom, searchPurpose, statusFilter, pageNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error cancelling booking {0}: {1}", id, ex.Message);
                TempData["Error"] = $"Error cancelling booking: {ex.Message}";
                return RedirectToAction("Index", new { searchRoom, searchPurpose, statusFilter, pageNumber });
            }
        }
    }
}