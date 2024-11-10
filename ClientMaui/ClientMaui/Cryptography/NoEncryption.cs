using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientMaui.Cryptography
{
    public class NoEncryption : ICryptography
    {
        public string key { get; set; }

        public string GenerateKey()
        {
            return "";
        }

        public Task<string> Encrypt(string text)
        {
            return Task.FromResult(text);
        }

        public Task<string> Decrypt(string text, bool isIncoming = false)
        {
            return Task.FromResult(text);
        }
    }
}
