namespace ClientMaui.Cryptography
{
    interface ICryptography
    {

        public string GenerateKey(int length = 128);
        public string key { get; set; }
        public string Encrypt(string text);
        public string Decrypt(string text);
    }

    class CryptographyHelper
    {
        public static ICryptography GetCryptography(string friendlyString)
        {
            return friendlyString switch
            {
                "DES" => new DES(),
                //"AES" => new AES(),
                _ => throw new NotImplementedException()
            };

        }
    }
}
