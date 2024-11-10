using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<string> Encrypt(string text)
        {
            var passwordBytes = Convert.FromBase64String(key);

            // Set encryption settings
            var des = System.Security.Cryptography.TripleDES.Create();
            des.Padding = PaddingMode.PKCS7;
            des.Mode = CipherMode.ECB;
            var transform = des.CreateEncryptor(passwordBytes, null);
            return await CryptographyHelper.EncryptBySymmetric(transform, text);
        }

        public async Task<string> Decrypt(string encryptedMessage, bool isIncoming = false)
        {
            // Set decryption settings
            var passwordBytes = Convert.FromBase64String(key);
            var des = System.Security.Cryptography.TripleDES.Create();
            des.Padding = PaddingMode.PKCS7;
            des.Mode = CipherMode.ECB;
            var transform = des.CreateDecryptor(passwordBytes, null);
            return await CryptographyHelper.DecryptBySymmetric(transform, encryptedMessage);

        }
    }
}
