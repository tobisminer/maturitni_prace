namespace ClientMaui.Cryptography
{
    public interface ICryptography
    {

        public string GenerateKey();
        public string key { get; set; }
        public Task<string> Encrypt(string text);
        public Task<string> Decrypt(string text, bool isIncoming = false);
    }

    class CryptographyHelper
    {
        public static ICryptography GetCryptography(string friendlyString)
        {
            return friendlyString switch
            {
                "DES" => new DES(),
                "AES" => new AESInstance(),
                "Triple DES" => new TripleDES(),
                "RC4" => new RCFour(),
                "RSA" => new RSAInstance(),
                _ => throw new NotImplementedException()
            };

        }
    }
}
