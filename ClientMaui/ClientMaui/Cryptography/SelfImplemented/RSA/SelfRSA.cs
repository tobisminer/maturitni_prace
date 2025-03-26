using ClientMaui.Cryptography.SelfImplemented;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

using ClientMaui.API;
using ClientMaui.Cryptography;
using ClientMaui.Cryptography.SelfImplemented.RSA;
using ClientMaui.Database;
using ClientMaui.Database.Entities;
using ClientMaui.Entities;
using ClientMaui.Entities.Room;

using Newtonsoft.Json;

using RestSharp;

using static System.Net.Mime.MediaTypeNames;

namespace ClientMaui.Cryptography.SelfImplemented.RSA
{
    internal class SelfRSA
    {
        static List<BigInteger> splitNumberToSmallerPowers(BigInteger number)
        {
            var result = new List<BigInteger>();
            var remainder = number;

            while (remainder > 0)
            {
                var mathLog = BigInteger.Log(remainder, 2);
                var power = (int)Math.Floor(mathLog);
                var value = BigInteger.Pow(2, power);
                result.Add(value);
                remainder -= value;
            }
            return result;

        }
        static BigInteger modPow(BigInteger number, BigInteger power, BigInteger mod)
        {
            //return BigInteger.ModPow(number, power, mod);
            var powers = splitNumberToSmallerPowers(power);
            var lookupTable = new Dictionary<BigInteger, BigInteger>();
            if (powers.Contains(1))
            {
                lookupTable.Add(1, number % mod);
            }

            for (var i = new BigInteger(2); i <= powers.Max(); i *= 2)
            {

                lookupTable.Add(i, BigInteger.Pow(lookupTable[i / 2], 2) % mod);

            }
            var result = powers.Aggregate(BigInteger.One, (current, item) => current * lookupTable[item]);

            return result % mod;
        }
        static BigInteger GenerateRandomBigInteger(int bitLength)
        {
            var byteLength = (bitLength + 7) / 8;
            var bytes = new byte[byteLength];
            RandomNumberGenerator.Fill(bytes);

            // Zajistíme, že nejvyšší bit je nastaven, aby číslo mělo správnou bitovou délku
            bytes[^1] |= 0x80;

            return new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        }

        // Miller-Rabin test na prvočíselnost
        static bool IsPrime(BigInteger n, int k = 10)
        {
            if (n < 2) return false;
            if (n == 2 || n == 3) return true;
            if (n % 2 == 0) return false;

            // Rozložíme n-1 na d * 2^r
            var d = n - 1;
            var r = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }

            // Opakujeme test k-krát
            for (var i = 0; i < k; i++)
            {
                var a = (GenerateRandomBigInteger(n.ToByteArray().Length * 8) % (n - 3)) + 2;
                var x = BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1) continue;

                var composite = true;
                for (var j = 0; j < r - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, n);

                    if (x != n - 1) continue;
                    composite = false;
                    break;
                }
                if (composite) return false;
            }
            return true;
        }

        // Generování prvočísla o dané bitové délce
        static BigInteger GeneratePrime(int bitLength)
        {
            BigInteger prime;
            do
            {
                prime = GenerateRandomBigInteger(bitLength) | 1; // Ujistíme se, že číslo je liché
            } while (!IsPrime(prime));
            return prime;
        }

        //Rozšířený Euklidův algoritmus
        static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m, t, q;
            BigInteger x0 = 0, x1 = 1;

            if (m == 1) return 0;

            while (a > 1)
            {
                q = a / m;
                t = m;

                m = a % m;
                a = t;
                t = x0;

                x0 = x1 - (q * x0);
                x1 = t;
            }

            if (x1 < 0)
                x1 += m0;

            return x1;
        }
        static BigInteger gcd(BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                (b, a) = (a % b, b);
            }
            return a;
        }

        public static (BigInteger E, BigInteger N, BigInteger D) GenerateRSAKeyPair(int keySize = 1024)
        {
            //GC.TryStartNoGCRegion(keySize * 1000 * 1000, keySize);
            var size = keySize / 2;
            var P = GeneratePrime(size);
            var Q = GeneratePrime(size);
            while (P == Q)
            {
                Q = GeneratePrime(size);
            }
            var N = P * Q;
            var phi = (P - 1) * (Q - 1);
            var E = new BigInteger(65537);
            var D = new BigInteger(0);
            while (true)
            {
                if (gcd(E, phi) == 1)
                {
                    D = ModInverse(E, phi);
                    break;
                }
                E++;
            }
            //GC.EndNoGCRegion();
            return (E, N, D);
        }
        static BigInteger EncryptCharacter(int message, BigInteger E, BigInteger N)
        {
            return modPow(message, E, N);
        }
        static BigInteger DecryptCharacter(BigInteger message, BigInteger D, BigInteger N)
        {
            return modPow(message, D, N);
        }
        public static string Encrypt(string message, BigInteger E, BigInteger N)
        {
            var result = message.Select(character => EncryptCharacter(character, E, N)).ToList();
            var text = string.Join(" ", result);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        }
        public static string Decrypt(string message, BigInteger D, BigInteger N)
        {
            var messageBytes = Convert.FromBase64String(message);
            var text = Encoding.UTF8.GetString(messageBytes);
            var messageArray = text.Split(" ").Select(BigInteger.Parse).ToList();
            var result = messageArray.Select(character => DecryptCharacter(character, D, N)).ToList();

            var bytes = result.Select(bi => (char)(int)bi).ToArray();
            return new string(bytes);
        }
    }
}
class SelfRSACryptography : ICryptography, IAsymmetricCypherSetup
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
        var (E, N, D) = SelfRSA.GenerateRSAKeyPair();
        publicKey = $"{E}|{N}";
        privateKey = $"{D}|{N}";
        Database.AddValueToSecureStorage("PublicKey", publicKey, endpoint.username,
            room.id);
        Database.AddValueToSecureStorage("PrivateKey", privateKey, endpoint.username, room.id);

        return key;
    }
    public async Task<bool> LoadKey()
    {
        if (string.IsNullOrEmpty(publicKey) == false && string.IsNullOrEmpty(privateKey) == false)
            return true;


        var publicKeyString = await Database.GetValueFromSecureStorage("PublicKey", endpoint.username, room.id);
        var privateKeyString = await Database.GetValueFromSecureStorage("PrivateKey", endpoint.username, room.id);

        if (string.IsNullOrEmpty(publicKeyString) || string.IsNullOrEmpty(privateKeyString))
            return false;

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
       var split = publicKey.Split("|");
        var E = BigInteger.Parse(split[0]);
        var N = BigInteger.Parse(split[1]);
        return SelfRSA.Encrypt(text, E, N);
    }
    public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
    {
        if (otherPublicKey == null)
        {
            var result = await GetOtherPublicKey();
            if (!result)
                return text;
        }
        var split = otherPublicKey.Split("|");
        var E = BigInteger.Parse(split[0]);
        var N = BigInteger.Parse(split[1]);
        var cypherString = SelfRSA.Encrypt(text, E, N);

        var mess = new MessageDbEntity
        {
            Message = EncryptByMyPublicKey(text),
            EncryptedMessage = cypherString
        };
        Database.AddMessage(mess);

        return SelfRSA.Encrypt(text, E, N);
    }
    public async Task<string> Decrypt(string encryptedMessage, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
    {
        if (!isIncoming) // message is coming from me
        {
            var mess = await Database.GetMessagesByEncryptedString(encryptedMessage);
            encryptedMessage = mess?.Message ?? encryptedMessage;
        }
        await LoadKey();
        var split = privateKey.Split("|");
        var D = BigInteger.Parse(split[0]);
        var N = BigInteger.Parse(split[1]);
        return SelfRSA.Decrypt(encryptedMessage, D, N);
    }

    public static async Task SetupForRsa(Endpoint endpoint,
        Room room,
        SelfRSACryptography instance)
    {
        instance.endpoint = endpoint;
        instance.room = room;
        _ = await instance.GetOtherPublicKey();

        if (await instance.LoadKey()) return;
        var myNewPublicKey = instance.GenerateKey();
        var myPublicKeyJson = new Key
        {
            key = Base64Encode(myNewPublicKey)
        };
        await endpoint.Request(APIEndpoints.RoomEndpoints.SetKey, body: JsonConvert.SerializeObject(myPublicKeyJson), method: Method.Post, id: room.id);
    }
    public async Task Setup(Endpoint endpoint, Room room)
    {
        await SetupForRsa(endpoint, room, this);
    }
}