﻿using ClassroomBooking.Repository.Entities;
using ClassroomBooking.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClassroomBooking.Presentation.Pages.Manager.Rooms
{
    [Authorize(Roles = "Manager")]
    public class EditModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;

        public EditModel(IRoomService roomService, ICampusService campusService)
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

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Room = await _roomService.GetRoomByIdAsync(id.Value);
            if (Room == null)
                return NotFound();

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
            Console.WriteLine($"Room.CampusId: {Room.CampusId}, Room.Status: {Room.Status}");

            ModelState.Remove("Room.Campus");
            ModelState.Remove("Room.RoomSlots");

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

            // Kiểm tra trạng thái có nằm trong danh sách hợp lệ không
            if (!new[] { "Available", "Occupied", "Maintenance" }.Contains(Room.Status))
            {
                ErrorMessage = "Please select a valid status.";
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
                await _roomService.UpdateRoomAsync(Room);
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error updating room: " + ex.Message;
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