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
            var messageBytes = Encoding.UTF8.GetBytes(text);
            var passwordBytes = Convert.FromBase64String(key);

            // Set encryption settings
            var des = System.Security.Cryptography.TripleDES.Create();
            des.Padding = PaddingMode.PKCS7;
            des.Mode = CipherMode.ECB;
            var transform = des.CreateEncryptor(passwordBytes, null);
            var mode = CryptoStreamMode.Write;

            // Set up streams and encrypt
            var memStream = new MemoryStream();
            var cryptoStream =
                new CryptoStream(memStream,
                    transform, mode);
            await cryptoStream.WriteAsync(messageBytes, 0, messageBytes.Length);
            await cryptoStream.FlushFinalBlockAsync();

            // Read the encrypted message from the memory stream
            var encryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            _ = await memStream.ReadAsync(encryptedMessageBytes, 0,
                encryptedMessageBytes.Length);

            // Encode the encrypted message as base64 string
            var encryptedMessage =
                Convert.ToBase64String(encryptedMessageBytes);

            return $"{encryptedMessage}";
        }

        public async Task<string> Decrypt(string text, bool isIncoming = false)
        {
            var encryptedMessage = Convert.FromBase64String(text);

            // Set decryption settings
            var des = System.Security.Cryptography.TripleDES.Create();
            des.Padding = PaddingMode.PKCS7;
            des.Mode = CipherMode.ECB;
            var transform =
                des.CreateDecryptor(Convert.FromBase64String(key), null);
            var mode = CryptoStreamMode.Write;

            // Set up streams and decrypt
            var memStream = new MemoryStream();
            var cryptoStream =
                new CryptoStream(memStream,
                    transform, mode);
            await cryptoStream.WriteAsync(encryptedMessage, 0, encryptedMessage.Length);
            await cryptoStream.FlushFinalBlockAsync();

            var decryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            _ = memStream.Read(decryptedMessageBytes, 0,
                decryptedMessageBytes.Length);

            var message = Encoding.UTF8.GetString(decryptedMessageBytes);

            return message;

        }
    }
}
