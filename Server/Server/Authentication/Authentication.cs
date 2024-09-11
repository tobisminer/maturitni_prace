using Microsoft.Identity.Client;
using Server.Data;

namespace Server
{
    public class Authentication
    {
        public static string? GetIdentifierFromToken(ApplicationDbContext db, string token)
        {
            var tokenEntity = db.Tokens.FirstOrDefault(x => x.token == token);
            return tokenEntity?.active == false ? null : tokenEntity?.identifier;
        }
        public static string GetTokenFromHeader(string header)
        {
            return header.Replace("Bearer ", "");
        }

    }
}
