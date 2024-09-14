using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR
{
    public static class HubExtension
    {
        public static void SendToUser(this IHubContext<NewMessageHub> hubContext, string? user, string message)
        {
            if(user == null) return;
            if (NewMessageHub.connectionToken.TryGetValue(user, out var value))
            {
                hubContext.Clients.Client(value).SendAsync("ReceiveMessage", message);
            }
        }
    }
} 