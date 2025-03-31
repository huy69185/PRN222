namespace ClassroomBooking.Service.Dtos
{
    public class RoomSlotDto
    {
        public int RoomSlotId { get; set; }
        public RoomDto? Room { get; set; }
        public int SeatsBooked { get; set; }
        public DateTime SlotDate { get; set; }
    }
}
