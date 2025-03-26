using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientMaui.API;
using ClientMaui.Cryptography.SelfImplemented.DES;
using ClientMaui.Entities.Room;

namespace ClientMaui.Cryptography.SelfImplemented.RSA
{
    internal class SelfRSAandSelfDES : ICryptography, IAsymmetricCypherSetup
    {
        public string key
        {
            get => RSA.key;
            set => RSA.key = value;
        } // This is the string public key

        public SelfRSACryptography RSA = new();
        public SelfDesCryptography DES = new();

        private const string SPLITTER = "AESKEY@CYPHERTEXT";

        public string GenerateKey()
        {
            var rsaKey = RSA.GenerateKey();
            key = rsaKey;
            RSA.key = rsaKey;
            return key;
        }

        public string GenerateDesKey()
        {
            DES.key = DES.GenerateKey();
            return DES.key;
        }

        public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
        {
            GenerateDesKey();
            var encryptedText = await DES.Encrypt(text, mode);
            var encryptedKey = await RSA.Encrypt(DES.key);
            return $"{encryptedKey}{SPLITTER}{encryptedText}";
        }

        public async Task<string> Decrypt(string text, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
        {
            var split = text.Split(SPLITTER);
            var decryptedKey = await RSA.Decrypt(split[0], mode, isIncoming);
            DES.key = decryptedKey;
            return await DES.Decrypt(split[1]);
        }
        public async Task Setup(Endpoint endpoint, Room room)
        {
            await SelfRSACryptography.SetupForRsa(endpoint, room, RSA);
        }
    }
}
