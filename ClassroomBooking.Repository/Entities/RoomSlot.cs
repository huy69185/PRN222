namespace ClassroomBooking.Repository.Entities
{
    public partial class RoomSlot
    {
        public int RoomSlotId { get; set; }
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int SeatsBooked { get; set; }
        public DateTime SlotDate { get; set; }

        public virtual Booking Booking { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
    }
}