using ClientMaui.API;
using ClientMaui.Database.Entities;
using ClientMaui.Entities.Room;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using ClientMaui.Entities;

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

            _ = Database.Database.AddValueToSecureStorage("PublicKey", publicKey, endpoint.username,
                room.id);
            _ = Database.Database.AddValueToSecureStorage("PrivateKey", privateKey, endpoint.username, room.id);

            return key;
        }

        public async Task<bool> LoadKey()
        {
            var publicKeyString = await Database.Database.GetValueFromSecureStorage("PublicKey", endpoint.username, room.id);
            var privateKeyString = await Database.Database.GetValueFromSecureStorage("PrivateKey", endpoint.username, room.id);

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

        private string EncryptByMyPublicKey(string text)
        {
            var csp = new RSACryptoServiceProvider();
            csp.FromXmlString(publicKey);
            var data = Encoding.UTF8.GetBytes(text);
            var cypher = csp.Encrypt(data, false);
            return Convert.ToBase64String(cypher);
        }

        public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
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
                Message = EncryptByMyPublicKey(text),
                EncryptedMessage = cypherString
            };
            Database.Database.AddMessage(mess);
            return cypherString;
        }

        public async Task<string> Decrypt(string text, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
        {
            if (!isIncoming)
            {
                var mess = await Database.Database.GetMessagesByEncryptedString(text);
                text = mess?.Message ?? text;
            }

            try
            {
                var csp = new RSACryptoServiceProvider();
                csp.FromXmlString(privateKey);
                var dataBytes = Convert.FromBase64String(text);
                var plainText = csp.Decrypt(dataBytes, false);
                return Encoding.UTF8.GetString(plainText);
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                //throw new Exception("Decryption failed. Key might not exist or be invalid.", ex);
            }
            return text;
        }

        public static async Task setupForRSA(Endpoint endpoint,
            Room room,
            RSAInstance rsaInstance)
        {
            rsaInstance.endpoint = endpoint;
            rsaInstance.room = room;
            _ = await rsaInstance.GetOtherPublicKey();

            if (await rsaInstance.LoadKey()) return;
            var myNewPublicKey = rsaInstance.GenerateKey();
            var myPublicKeyJson = new Key
            {
                key = Base64Encode(myNewPublicKey)
            };
            await endpoint.Request(APIEndpoints.RoomEndpoints.SetKey, body: JsonConvert.SerializeObject(myPublicKeyJson), method: Method.Post, id: room.id);
        }

    }

}
