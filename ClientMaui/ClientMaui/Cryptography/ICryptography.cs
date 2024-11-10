using ClientMaui.Entities.Room;
using System.Security.Cryptography;
using System.Text;

namespace ClientMaui.Cryptography
{
    public interface ICryptography
    {

        public string GenerateKey();
        public string key { get; set; }
        public Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None);
        public Task<string> Decrypt(string text, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false);
    }

    public class CryptographyHelper
    {
        public static ICryptography GetCryptography(string friendlyString)
        {
            return friendlyString switch
            {
                "No Encryption" => new NoEncryption(),
                "DES" => new DES(),
                "AES" => new AESInstance(),
                "Triple DES" => new TripleDES(),
                "RC4" => new RCFour(),
                "RSA" => new RSAInstance(),
                "RSA+AES" => new RSAandAES(),
                _ => new NoEncryption()
            };

        }
        public static bool BlockCypherMode(string friendlyString)
        {
            return friendlyString switch
            {
                "No Encryption" => false,
                "DES" => true,
                "AES" => true,
                "Triple DES" => true,
                "RC4" => false,
                "RSA" => false,
                "RSA+AES" => false,
                _ => false
            };
        }

        public static (byte[], string) DivideMessage(string message)
        {
            var split = message.Split("|");
            return (Convert.FromBase64String(split[0]), split[1]);
        }
        public static async Task<string> EncryptSymmetric(ICryptoTransform transform, string message, byte[] IV)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var IVString = Convert.ToBase64String(IV);
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
            return $"{IVString}|{encryptedMessage}";

        }
        public static async Task<string> DecryptSymmetric(ICryptoTransform transform, string encryptedMessage)
        {
            var encryptedMessageBytes = Convert.FromBase64String(encryptedMessage);
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
