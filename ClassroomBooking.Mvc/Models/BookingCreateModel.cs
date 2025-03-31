namespace ClassroomBooking.Mvc.Models
{
    public class BookingCreateModel
    {
        public int StudentId { get; set; }
        public string Purpose { get; set; }
        public int SelectedCampusId { get; set; }
        public int SelectedRoomId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int SeatsWanted { get; set; }
        public int CapacityLeft { get; set; } // Thêm thuộc tính CapacityLeft
    }
}