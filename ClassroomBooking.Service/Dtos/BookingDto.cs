namespace ClassroomBooking.Service.Dtos
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int StudentId { get; set; }

        // Thêm RoomSlots nếu bạn muốn hiển thị
        public List<RoomSlotDto> RoomSlots { get; set; } = new List<RoomSlotDto>();

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = "Pending";
    }
}
