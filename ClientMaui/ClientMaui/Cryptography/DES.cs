using ClientMaui.Entities.Room;

namespace ClientMaui.Cryptography
{
    internal class DES : ICryptography
    {

        public string key { get; set; }


        public string GenerateKey()
        {
            byte[] key;

            var des = System.Security.Cryptography.DES.Create();
            key = des.Key;

            return Convert.ToBase64String(key);
        }

        public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
        {
            var cypher = System.Security.Cryptography.DES.Create();
            var IV = cypher.IV;
            var transform =
                CryptographyHelper.CreateSymmetricEncryptor(cypher, key, IV, mode);
            return await CryptographyHelper.EncryptSymmetric(transform, text, IV);
        }

        public async Task<string> Decrypt(string encryptedMessage, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
        {
            var (IV, message) =
                CryptographyHelper.DivideMessage(encryptedMessage);
            var cypher = System.Security.Cryptography.DES.Create();
            var transform =
                CryptographyHelper.CreateSymmetricDecryptor(cypher, key, IV, mode);
            return await CryptographyHelper.DecryptSymmetric(transform, message);
        }
    }
}
