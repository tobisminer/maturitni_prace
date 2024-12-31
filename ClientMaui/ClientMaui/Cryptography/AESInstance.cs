using ClientMaui.Entities.Room;
using System.Security.Cryptography;


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

        public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
        {
            // Set encryption settings
            var cypher = Aes.Create();
            var IV = cypher.IV;
            var transform =
                CryptographyHelper.CreateSymmetricEncryptor(cypher, key, IV, mode);
            return await CryptographyHelper.EncryptSymmetric(transform, text, IV);
        }

        public async Task<string> Decrypt(string encryptedMessage, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
        {
            var (IV, message) =
                CryptographyHelper.DivideMessage(encryptedMessage);
            var cypher = Aes.Create();
            var transform =
                CryptographyHelper.CreateSymmetricDecryptor(cypher, key, IV, mode);
            return await CryptographyHelper.DecryptSymmetric(transform, message);
        }


    }
}
