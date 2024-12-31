using ClientMaui.Entities.Room;
using System.Security.Cryptography;


namespace ClientMaui.Cryptography
{
    internal class AESInstance : ICryptography
    {
        public string key { get; set; }

        public string GenerateKey()
        {
            var aes = Aes.Create();
            var tempKey = aes.Key;

            return Convert.ToBase64String(tempKey);
        }

        public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
        {
            var cypher = Aes.Create();
            return await CryptographyHelper.EncryptSymmetric(cypher, key, mode, text);
        }

        public async Task<string> Decrypt(string encryptedMessage, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
        {
            var cypher = Aes.Create();
            return await CryptographyHelper.DecryptSymmetric(cypher, key, mode, encryptedMessage);
        }


    }
}
