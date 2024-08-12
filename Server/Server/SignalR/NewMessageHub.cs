using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR
{
    public sealed class NewMessageHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client {Context.ConnectionId} connected");

            //Return the connection its ID/Token
            Clients.Client(Context.ConnectionId).SendAsync("ReceiveId", Context.ConnectionId);

            return base.OnConnectedAsync();
        }
    }
}
