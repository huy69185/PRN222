using Microsoft.AspNetCore.SignalR;

namespace ClassroomBooking.Service.Hubs
{
    public class BookingHub : Hub
    {
        public async Task SendBookingNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveBookingNotification", message);
        }
    }
}