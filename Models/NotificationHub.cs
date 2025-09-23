namespace TMS.Models
{
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;
    public class NotificationHub:Hub
    {
        public async Task SendNotification(string userId, string message, string link)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message, link);
        }
    }
}
