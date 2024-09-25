using System.Security.Cryptography;
using System.Text;

namespace ClientMaui.Cryptography
{
    internal class DES : ICryptography
    {

        public string key { get; set; }

        
        public string GenerateKey(int length = 128)
        {

            byte[] key;

            var des = System.Security.Cryptography.DES.Create();
            key = des.Key;
            
            return Convert.ToBase64String(key);

        }

        private byte[] GenerateRandomIV(int length)
        {
            var des = System.Security.Cryptography.DES.Create();
            var iv = des.IV;
            return iv;
        }

        public string Encrypt(string text)
        {
            var messageBytes = Encoding.ASCII.GetBytes(text);
            var passwordBytes = Convert.FromBase64String(key);
            var iv = GenerateRandomIV(passwordBytes.Length);

            // Set encryption settings
            var des = System.Security.Cryptography.DES.Create();
            des.Padding = PaddingMode.PKCS7;
            var transform = des.CreateEncryptor(passwordBytes, iv);
            var mode = CryptoStreamMode.Write;

            // Set up streams and encrypt
            var memStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memStream, transform, mode);
            cryptoStream.Write(messageBytes, 0, messageBytes.Length);
            cryptoStream.FlushFinalBlock();

            // Read the encrypted message from the memory stream
            var encryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            _ = memStream.Read(encryptedMessageBytes, 0, encryptedMessageBytes.Length);

            // Encode the encrypted message as base64 string
            var encryptedMessage = Convert.ToBase64String(encryptedMessageBytes);
            var ivString = Convert.ToBase64String(iv);

            return $"{ivString}|{encryptedMessage}";
        }

        public string Decrypt(string encryptedMessage)
        {
            // Convert encrypted message and password to bytes
            
            var encryptedMessageBytes = Convert.FromBase64String(encryptedMessage.Split('|')[1]);
            var ivBytes = Convert.FromBase64String(encryptedMessage.Split('|')[0]);

            var passwordBytes = Convert.FromBase64String(key);

            // Set encryption settings 
            var des = System.Security.Cryptography.DES.Create();
            des.Padding = PaddingMode.PKCS7;
            var transform = des.CreateDecryptor(passwordBytes, ivBytes);
            var mode = CryptoStreamMode.Write;

            // Set up streams and decrypt
            var memStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memStream, transform, mode);
            cryptoStream.Write(encryptedMessageBytes, 0, encryptedMessageBytes.Length);
            cryptoStream.FlushFinalBlock();

            // Read decrypted message from memory stream
            var decryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            _= memStream.Read(decryptedMessageBytes, 0, decryptedMessageBytes.Length);

            var message = Encoding.ASCII.GetString(decryptedMessageBytes);

            return message;
        }
    }
}
