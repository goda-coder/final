// في مجلد Hubs/NotificationHub.cs
using Microsoft.AspNetCore.SignalR;

namespace final.Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
    }
}