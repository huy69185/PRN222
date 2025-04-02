using Microsoft.AspNetCore.SignalR;

namespace ClassroomBooking.Service.Hubs
{
    public class BookingHub : Hub
    {
        // Thông báo cho các hành động liên quan đến booking
        public async Task SendBookingNotification(string action, string message)
        {
            await Clients.All.SendAsync("ReceiveBookingNotification", action, message);
        }

        // Thông báo cho các hành động liên quan đến phòng
        public async Task SendRoomNotification(string action, string message)
        {
            await Clients.All.SendAsync("ReceiveRoomNotification", action, message);
        }
    }
}