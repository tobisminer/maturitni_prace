using ClientMaui.Entities.Room;

namespace ClientMaui.Cryptography
{
    internal class RSAandAES : ICryptography
    {
        public string key
        {
            get => RSA.key;
            set => RSA.key = value;
        } // This is the string public key

        public RSAInstance RSA = new();
        public AESInstance Aes = new();

        private const string SPLITTER = "AESKEY@CYPHERTEXT";

        public string GenerateKey()
        {
            var rsaKey = RSA.GenerateKey();
            key = rsaKey;
            RSA.key = rsaKey;
            return key;
        }

        public string GenerateAesKey()
        {
            Aes.key = Aes.GenerateKey();
            return Aes.key;
        }

        public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
        {
            GenerateAesKey();
            var encryptedText = await Aes.Encrypt(text);
            var encryptedKey = await RSA.Encrypt(Aes.key);
            return $"{encryptedKey}{SPLITTER}{encryptedText}";
        }

        public async Task<string> Decrypt(string text, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
        {
            var split = text.Split(SPLITTER);
            var decryptedKey = await RSA.Decrypt(split[0], mode, isIncoming);
            Aes.key = decryptedKey;
            return await Aes.Decrypt(split[1]);
        }
    }
}
