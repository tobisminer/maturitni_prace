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
            var messageBytes = Encoding.UTF8.GetBytes(text);
            var passwordBytes = Convert.FromBase64String(key);

            // Set encryption settings
            var aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.ECB;
            var transform = aes.CreateEncryptor(passwordBytes, null);
            var mode = CryptoStreamMode.Write;

            // Set up streams and encrypt
            var memStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memStream, transform, mode);
            await cryptoStream.WriteAsync(messageBytes, 0, messageBytes.Length);
            await cryptoStream.FlushFinalBlockAsync();

            // Read the encrypted message from the memory stream
            var encryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            _ = memStream.Read(encryptedMessageBytes, 0, encryptedMessageBytes.Length);

            // Encode the encrypted message as base64 string
            var encryptedMessage = Convert.ToBase64String(encryptedMessageBytes);

            return $"{encryptedMessage}";
        }

        public async Task<string> Decrypt(string encryptedMessage, bool isIncoming = false)
        {
            var encryptedMessageBytes = Convert.FromBase64String(encryptedMessage);
           

            var passwordBytes = Convert.FromBase64String(key);

            // Set encryption settings 
            var des = System.Security.Cryptography.Aes.Create();
            des.Padding = PaddingMode.PKCS7;
            des.Mode = CipherMode.ECB;
            var transform = des.CreateDecryptor(passwordBytes, null);
            var mode = CryptoStreamMode.Write;

            // Set up streams and decrypt
            var memStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memStream, transform, mode);
            await cryptoStream.WriteAsync(encryptedMessageBytes, 0, encryptedMessageBytes.Length);
            await cryptoStream.FlushFinalBlockAsync();

            // Read decrypted message from memory stream
            var decryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            _ = memStream.Read(decryptedMessageBytes, 0, decryptedMessageBytes.Length);

            var message = Encoding.UTF8.GetString(decryptedMessageBytes);

            return message;
        }


    }
}
