using System.Security.Cryptography;
using System.Text;

namespace Server.Cryptography
{
    public class LocalHash
    {
        public static string Sha256(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var inputHash = SHA256.HashData(inputBytes);
            return Convert.ToHexString(inputHash);
        }
    }
}
