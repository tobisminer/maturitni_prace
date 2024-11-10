using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ClientMaui.Cryptography
{
    internal class AESInstance : ICryptography
    {
        public string key { get; set; }

        public string GenerateKey()
        {
            byte[] key;

            var aes = Aes.Create();
            key = aes.Key;

            return Convert.ToBase64String(key);

        }

        public async Task<string> Encrypt(string text)
        {
            var passwordBytes = Convert.FromBase64String(key);
            // Set encryption settings
            var aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.ECB;
            var transform = aes.CreateDecryptor(passwordBytes, null); 
            return await CryptographyHelper.EncryptBySymmetric(transform, text);
        }

        public async Task<string> Decrypt(string encryptedMessage, bool isIncoming = false)
        { 
            var passwordBytes = Convert.FromBase64String(key);
           // Set encryption settings
            var aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.ECB;
            var transform = aes.CreateDecryptor(passwordBytes, null);
            return await CryptographyHelper.DecryptBySymmetric(transform, encryptedMessage);
        }


    }
}
