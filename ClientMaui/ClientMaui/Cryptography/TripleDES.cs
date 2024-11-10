using ClientMaui.Entities.Room;
using System.Security.Cryptography;

namespace ClientMaui.Cryptography
{
    internal class TripleDES : ICryptography
    {
        public string key { get; set; }

        public string GenerateKey()
        {
            byte[] key;

            var des = System.Security.Cryptography.TripleDES.Create();
            key = des.Key;

            return Convert.ToBase64String(key);

        }
        public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
        {
            var passwordBytes = Convert.FromBase64String(key);

            // Set encryption settings
            var des = System.Security.Cryptography.TripleDES.Create();
            var IV = des.IV;
            des.Padding = PaddingMode.PKCS7;
            des.Mode = BlockCypherModeHelper.ConvertToCipherMode(mode);
            var transform = des.CreateEncryptor(passwordBytes, IV);
            return await CryptographyHelper.EncryptSymmetric(transform, text, IV);
        }

        public async Task<string> Decrypt(string encryptedMessage, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
        {
            var passwordBytes = Convert.FromBase64String(key);
            var (IV, message) =
                CryptographyHelper.DivideMessage(encryptedMessage);

            // Set encryption settings 
            var des = System.Security.Cryptography.TripleDES.Create();
            des.Padding = PaddingMode.PKCS7;
            des.Mode = BlockCypherModeHelper.ConvertToCipherMode(mode);
            var transform = des.CreateDecryptor(passwordBytes, IV);
            return await CryptographyHelper.DecryptSymmetric(transform, message);
        }
    }
}
