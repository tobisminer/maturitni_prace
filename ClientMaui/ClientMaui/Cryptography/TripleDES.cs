using ClientMaui.Entities.Room;

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
        public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
        {
            var cypher = System.Security.Cryptography.TripleDES.Create();
            return await CryptographyHelper.EncryptSymmetric(cypher, key, mode, text);
        }

        public async Task<string> Decrypt(string encryptedMessage, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
        {
            var cypher = System.Security.Cryptography.TripleDES.Create();
            return await CryptographyHelper.DecryptSymmetric(cypher, key, mode, encryptedMessage);
        }
    }
}
