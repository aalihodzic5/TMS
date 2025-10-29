namespace TMS.Models
{
    using Microsoft.AspNetCore.SignalR;
    using System.Security.Claims;
    using System.Threading.Tasks;
    public class NotificationHub:Hub
    {
        public async Task SendNotificationToUser(string recipientUserId, string message, string link)
        {
            var senderUserId = Context.UserIdentifier; 

            await Clients.User(recipientUserId).SendAsync("ReceiveNotification",senderUserId, message, link);
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;

            if (user == null)
            {
                await base.OnConnectedAsync();
                return;
            }

            string? userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                await base.OnConnectedAsync();
                return;
            }

            Connections.Add(Context.ConnectionId, userId);

            await base.OnConnectedAsync();
        }


        public static class Connections
        {
            public static Dictionary<string, string> Users = new Dictionary<string, string>();
            public static void Add(string connectionId, string userName)
            {
                Users[connectionId] = userName;
            }

            public static string? GetUserName(string connectionId)
            {
                if (Users.ContainsKey(connectionId))
                {
                    return Users[connectionId];
                }
                return null;
            }
        }


    }
}
