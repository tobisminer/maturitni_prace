using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ClientMaui.API;
using ClientMaui.Database;
using ClientMaui.Database.Entities;
using ClientMaui.Entities.Room;

namespace ClientMaui.Cryptography
{
    public class RSAInstance : ICryptography
    {
        public string key
        {
            get => publicKey;
            set => publicKey = value;
        } // This is the string public key
        
        private string publicKey { get; set; }
        private string privateKey { get; set; }

        public string? otherPublicKey { get; set; }

        public Endpoint endpoint { get; set; }
        public Room room { get; set; }

        public string GenerateKey()
        {
            var csp = new RSACryptoServiceProvider(2048);

            
            publicKey = csp.ToXmlString(false);
            privateKey = csp.ToXmlString(true);
           
            Preferences.Default.Set(room.id + endpoint.username +  "PUBLIC", publicKey);
            Preferences.Default.Set(room.id + endpoint.username + "PRIVATE", privateKey);

            return key;
        }

        public bool LoadKey()
        {
            var publicKeyString = Preferences.Default.Get(room.id + endpoint.username + "PUBLIC", "");
            var privateKeyString = Preferences.Default.Get(room.id + endpoint.username + "PRIVATE", "");

            if (string.IsNullOrEmpty(publicKeyString) || string.IsNullOrEmpty(privateKeyString))
            {
                return false;
            }

            publicKey = publicKeyString;
            privateKey = privateKeyString;

            return true;
        }
        
        public async Task<bool> GetOtherPublicKey()
        {
            var otherPublicKeyServer = (await endpoint.Request(APIEndpoints.RoomEndpoints.GetKey, id: room.id)).Content;
            if (string.IsNullOrEmpty(otherPublicKeyServer))
            {
                return false;
            }
            otherPublicKey = Base64Decode(otherPublicKeyServer);
            return true;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            base64EncodedData =
                base64EncodedData.Substring(1, base64EncodedData.Length - 2);
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public async Task<string> Encrypt(string text)
        {
            if (otherPublicKey == null)
            {
                var result = await GetOtherPublicKey();
                if (!result)
                    return text;
            }
            var csp = new RSACryptoServiceProvider();
            csp.FromXmlString(otherPublicKey);
            var data = Encoding.UTF8.GetBytes(text);
            var cypher = csp.Encrypt(data, false);
            var cypherString = Convert.ToBase64String(cypher);
            var mess = new MessageDbEntity
            {
               Message = text,
               EncryptedMessage = cypherString
            };
            Database.Database.AddMessage(mess);
            return cypherString;
        }

        public async Task<string> Decrypt(string text, bool isIncoming = false)
        {
            if (!isIncoming)
            {
                var mess = await Database.Database.GetMessagesByEncryptedString(text);
                return mess?.Message ?? text;
            }

            if (publicKey == otherPublicKey)
            {
                Console.WriteLine("What");
            }

            try
            {
                var csp = new RSACryptoServiceProvider();
                csp.FromXmlString(privateKey);
                var dataBytes = Convert.FromBase64String(text);
                var plainText = csp.Decrypt(dataBytes, false);
                return Encoding.UTF8.GetString(plainText);
            }
            catch (CryptographicException ex)
            {
                // Log or handle the exception as needed
                throw new Exception("Decryption failed. Key might not exist or be invalid.", ex);
            }
        }
    }
   
}
