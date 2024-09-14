using Microsoft.AspNetCore.SignalR;
using Server.Data;


namespace Server.SignalR
{
    public sealed class NewMessageHub(ApplicationDbContext db) : Hub
    {
        public static Dictionary<string, string> connectionToken = new();
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client {Context.ConnectionId} connected");

            var context = Context.GetHttpContext();
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var identifier = Authentication.GetIdentifierFromToken(db, token);
            if (identifier == null)
            {
                return base.OnConnectedAsync();
            }

            if (connectionToken.ContainsKey(identifier))
                connectionToken.Remove(identifier);
            connectionToken.Add(identifier, Context.ConnectionId);
            
            return base.OnConnectedAsync();
        }
    }
}
