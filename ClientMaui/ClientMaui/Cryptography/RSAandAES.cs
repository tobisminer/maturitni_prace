namespace ClientMaui.Cryptography
{
    internal class RSAandAES : ICryptography
    {
        public string key
        {
            get => rsa.key;
            set => rsa.key = value;
        } // This is the string public key

        public RSAInstance rsa = new();
        public AESInstance aes = new();

        public string GenerateKey()
        {
            var rsaKey = rsa.GenerateKey();
            key = rsaKey;
            rsa.key = rsaKey;
            return key;
        }

        public string GenerateAESKey()
        {
            aes.key = aes.GenerateKey();
            return aes.key;
        }

        public async Task<string> Encrypt(string text)
        {
            GenerateAESKey();
            var encryptedText = await aes.Encrypt(text);
            var encryptedKey = await rsa.Encrypt(aes.key);
            return $"{encryptedKey}|{encryptedText}";
        }

        public async Task<string> Decrypt(string text, bool isIncoming = false)
        {
            var split = text.Split('|');
            var decryptedKey = await rsa.Decrypt(split[0], isIncoming);
            aes.key = decryptedKey;
            return await aes.Decrypt(split[1]);
        }
    }
}
