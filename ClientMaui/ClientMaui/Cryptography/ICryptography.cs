using ClientMaui.Entities.Room;
using System.Security.Cryptography;
using System.Text;

using ClientMaui.Cryptography.SelfImplemented;
using ClientMaui.Cryptography.SelfImplemented.DES;
using ClientMaui.Cryptography.SelfImplemented.RSA;

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
                "Self DES" => new SelfDesCryptography(),
                "Self Triple DES" => new SelfTripleDesCryptography(),
                "Self AES" => new SelfAesCryptography(),
                "Self RSA" => new SelfRSACryptography(),

                _ => new NoEncryption()
            };

        }
        public static bool GetBlockCypherStatus(string friendlyString)
        {
            return friendlyString switch
            {
                "DES" => true,
                "AES" => true,
                "Triple DES" => true,
                "RSA+AES" => true,
                "Self DES" => true,
                "Self AES" => true,
                "Self Triple DES" => true,
                _ => false
            };
        }

        private const string DIVIDER = "IV@TEXT";

        public static (byte[], string) DivideMessage(string message)
        {
            var split = message.Split(DIVIDER);
            return (Convert.FromBase64String(split[0]), split[1]);
        }


        // Všechny symetrické šifry používají stejný interface, můžeme zde použít dynamic a všechny šifry schovat pod těchto pár jednotlivých metod a jednotlivé typy se
        // vytvářejí až v konkrétních třídách a celý kód je tak mnohem čitelnější a jednodušší na údržbu. Změnou toho kódu se změní fungování všech symetrických šifer.
        public static ICryptoTransform CreateSymmetricTransform(dynamic cypher, string key, byte[] IV, BlockCypherMode mode, bool encrypt)
        {
            var passwordBytes = Convert.FromBase64String(key);

            // Nastavení paddingu a módu šifrování
            cypher.Padding = PaddingMode.PKCS7;
            cypher.Mode = BlockCypherModeHelper.ConvertToCipherMode(mode);
            return encrypt ? cypher.CreateEncryptor(passwordBytes, IV) : cypher.CreateDecryptor(passwordBytes, IV);
        }

        public static async Task<string> EncryptSymmetric(dynamic cypher, string key, BlockCypherMode cypherMode, string message)
        {

            var transform =
                CreateSymmetricTransform(cypher, key, cypher.IV, cypherMode,
                                         true);
            var IV = cypher.IV;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            var ivString = Convert.ToBase64String(IV);
            const CryptoStreamMode mode = CryptoStreamMode.Write;

            // Vytvoření memory streamu a zápis do něj
            var memStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memStream, transform, mode);
            await cryptoStream.WriteAsync(messageBytes);
            await cryptoStream.FlushFinalBlockAsync();

            // Přečtení zašifrované zprávy z memory streamu
            var encryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            _ = await memStream.ReadAsync(encryptedMessageBytes);

            // Encode the encrypted message as base64 string
            var encryptedMessage = Convert.ToBase64String(encryptedMessageBytes);
            return $"{ivString}{DIVIDER}{encryptedMessage}";
        }
        public static async Task<string> DecryptSymmetric(dynamic cypher, string key, BlockCypherMode cypherMode, string encryptedMessage)
        {
            var (IV, message) =
                DivideMessage(encryptedMessage);

            var transform =
                CreateSymmetricTransform(cypher, key, IV, cypherMode, false);
            var encryptedMessageBytes = Convert.FromBase64String(message);
            const CryptoStreamMode mode = CryptoStreamMode.Write;

            // Vytvoření memory streamu a dešifrování
            var memStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memStream, transform, mode);
            await cryptoStream.WriteAsync(encryptedMessageBytes);
            await cryptoStream.FlushFinalBlockAsync();

            // Přečtení dešifrované zprávy z memory streamu
            var decryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            _ = await memStream.ReadAsync(decryptedMessageBytes);

            var decryptedMessage = Encoding.UTF8.GetString(decryptedMessageBytes);
            return decryptedMessage;
        }
    }
}
