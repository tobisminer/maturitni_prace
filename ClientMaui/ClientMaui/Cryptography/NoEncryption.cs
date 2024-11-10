using ClientMaui.Entities.Room;

namespace ClientMaui.Cryptography
{
    public class NoEncryption : ICryptography
    {
        public string key { get; set; }

        public string GenerateKey()
        {
            return "";
        }

        public Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
        {
            return Task.FromResult(text);
        }

        public Task<string> Decrypt(string text, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
        {
            return Task.FromResult(text);
        }
    }
}
