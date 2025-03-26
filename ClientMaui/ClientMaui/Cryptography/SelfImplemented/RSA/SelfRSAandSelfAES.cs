using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientMaui.API;
using ClientMaui.Entities.Room;

namespace ClientMaui.Cryptography.SelfImplemented.RSA
{
    internal class SelfRSAandSelfAES : ICryptography, IAsymmetricCypherSetup
    {
        public string key
        {
            get => RSA.key;
            set => RSA.key = value;
        } // This is the string public key

        public SelfRSACryptography RSA = new();
        public SelfAesCryptography AES = new();

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
            AES.key = AES.GenerateKey();
            return AES.key;
        }

        public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
        {
            GenerateAesKey();
            var encryptedText = await AES.Encrypt(text, mode);
            var encryptedKey = await RSA.Encrypt(AES.key);
            return $"{encryptedKey}{SPLITTER}{encryptedText}";
        }

        public async Task<string> Decrypt(string text, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
        {
            var split = text.Split(SPLITTER);
            var decryptedKey = await RSA.Decrypt(split[0], mode, isIncoming);
            AES.key = decryptedKey;
            return await AES.Decrypt(split[1]);
        }
        public async Task Setup(Endpoint endpoint, Room room)
        {
            await SelfRSACryptography.SetupForRsa(endpoint, room, RSA);
        }
    }
}
