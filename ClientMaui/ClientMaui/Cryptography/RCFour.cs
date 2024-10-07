using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClientMaui.Cryptography
{
    internal class RCFour : ICryptography
    {
        public string key { get; set; }


        private static readonly char[] _base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray();

        public static string GenerateRandomString(int length = 128)
        {
            var randomData = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomData);
            }

            var result = new StringBuilder(length);
            foreach (var b in randomData)
            {
                result.Append(_base64Chars[b % _base64Chars.Length]);
            }

            return result.ToString();
        }
        public string GenerateKey()
        {
            return GenerateRandomString();

        }

        public Task<string> Encrypt(string text)
        {
            var messageBytes = Encoding.UTF8.GetBytes(text);
            var passwordBytes = Encoding.UTF8.GetBytes(key);

            var encryptedMessageBytes = Apply(messageBytes, passwordBytes);
            var encryptedMessage = Convert.ToBase64String(encryptedMessageBytes);

            return Task.FromResult(encryptedMessage);
        }

        public Task<string> Decrypt(string text, bool isIncoming = false)
        {
            var encryptedMessageBytes = Convert.FromBase64String(text);
            var passwordBytes = Encoding.UTF8.GetBytes(key);

            var decryptedMessageBytes = Apply(encryptedMessageBytes, passwordBytes);
            var decryptedMessage = Encoding.UTF8.GetString(decryptedMessageBytes);

            return Task.FromResult(decryptedMessage);
        }

        public static byte[] Apply(byte[] data, byte[] key)
        {
            var S = Enumerable.Range(0, 256).ToArray();
            
            int i;
            var j = 0;
            for (i = 0; i < 256; i++)
            {
                j = (j + S[i] + key[i % key.Length]) % 256;
                (S[i], S[j]) = (S[j], S[i]);
            }

            i = j = 0;
            var result = new byte[data.Length];
            for (var iteration = 0; iteration < data.Length; iteration++)
            { 
                i = (i + 1) % 256;
                j = (j + S[i]) % 256;
                (S[i], S[j]) = (S[j], S[i]);
              
                var K = S[(S[i] + S[j]) % 256];

                result[iteration] = Convert.ToByte(data[iteration] ^ K);
            }

            //  return the result
            return result;
        }
    }

}


