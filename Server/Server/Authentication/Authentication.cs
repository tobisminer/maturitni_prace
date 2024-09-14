using Microsoft.Identity.Client;
using Server.Data;

namespace Server
{
    public class Authentication
    {
        public static string? GetIdentifierFromToken(ApplicationDbContext db, string token)
        {
            var tokens = db.Tokens.ToList();
            var tokenEntity = tokens.FirstOrDefault(tokenEntity => tokenEntity.token == token);
            return tokenEntity?.active == false ? null : tokenEntity?.identifier;
        }
        public static string GetTokenFromHeader(string header)
        {
            return header.Replace("Bearer ", "");
        }

    }
}
