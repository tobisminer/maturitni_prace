using System.Text;

using ClientMaui.Entities.Room;

namespace ClientMaui.Cryptography.SelfImplemented.DES;

class SelfTripleDesOverhead : Utils
{

    private static string IVConnectionString = "IV@CYPHERTEXT";

    public static (byte[] IV, string cyptherText) SplitIV(string text)
    {
        var split = text.Split(IVConnectionString);
        var ivBytes = Convert.FromBase64String(split[0]);
        return (ivBytes, split[1]);
    }

    public static string Encrypt(string input, byte[] keys)
    {
        if (keys.Length != 24)
        {
            throw new ArgumentException("Key length must be 24 bytes");
        }
        var (k1, k2, k3) = SplitKey(keys);
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes);
        var encryptedBlocks = new List<byte[]>();

        foreach (var block in blocks)
        {
            var firstEncryption = SelfDES.EncryptBlock(block, k1);
            var secondDecryption = SelfDES.DecryptBlock(firstEncryption, k2);
            var thirdEncryption = SelfDES.EncryptBlock(secondDecryption, k3);
            encryptedBlocks.Add(thirdEncryption);
        }
        return ByteListToString(encryptedBlocks);
    }
    public static string Decrypt(string input, byte[] keys)
    {
        if (keys.Length != 24)
        {
            throw new ArgumentException("Key length must be 24 bytes");
        }
        var (k1, k2, k3) = SplitKey(keys);
        var blocks = StringToByteList(input);
        var decryptedBlocks = new List<byte[]>();
        foreach (var block in blocks)
        {
            var firstDecryption = SelfDES.DecryptBlock(block, k3);
            var secondEncryption = SelfDES.EncryptBlock(firstDecryption, k2);
            var thirdDecryption = SelfDES.DecryptBlock(secondEncryption, k1);
            decryptedBlocks.Add(thirdDecryption);
        }
        decryptedBlocks = RemovePaddingFromList(decryptedBlocks);
        var output = ArrayListToString(decryptedBlocks);

        return output;
    }
    public static string EncryptCBC(string input, byte[] keys, byte[] iv)
    {
        if (keys.Length != 24)
        {
            throw new ArgumentException("Key length must be 24 bytes");
        }

        var (k1, k2, k3) = SplitKey(keys);
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes);
        var encryptedBlocks = new List<byte[]>();
        foreach (var block in blocks)
        {
            var firstEncryption = SelfDES.EncryptBlock(block, k1);
            var secondDecryption = SelfDES.DecryptBlock(firstEncryption, k2);
            encryptedBlocks.Add(secondDecryption);
        }
        encryptedBlocks = EncryptWithCBC(k3, iv, encryptedBlocks, SelfDES.EncryptBlock);
        var text = ByteListToString(encryptedBlocks);

        return $"{Convert.ToBase64String(iv)}{IVConnectionString}{text}";
    }
    public static string DecryptCBC(string input, byte[] keys)
    {
        if (keys.Length != 24)
        {
            throw new ArgumentException("Key length must be 24 bytes");
        }
        var (iv, cypherText) = SplitIV(input);
        var (k1, k2, k3) = SplitKey(keys);

        var blocks = StringToByteList(cypherText);

        var intermediateBlocks = DecryptWithCBC(k3, iv, blocks, SelfDES.DecryptBlock);

        var finalDecryptedBlocks = new List<byte[]>();
        foreach (var block in intermediateBlocks)
        {
            var secondEncryption = SelfDES.EncryptBlock(block, k2);
            var firstDecryption = SelfDES.DecryptBlock(secondEncryption, k1);
            finalDecryptedBlocks.Add(firstDecryption);
        }

        finalDecryptedBlocks = RemovePaddingFromList(finalDecryptedBlocks);
        return ArrayListToString(finalDecryptedBlocks);
    }
    public static string EncryptCFB(string input, byte[] keys, byte[] iv)
    {
        if (keys.Length != 24)
        {
            throw new ArgumentException("Key length must be 24 bytes");
        }

        var (k1, k2, k3) = SplitKey(keys);
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes);
        var encryptedBlocks = new List<byte[]>();
        foreach (var block in blocks)
        {
            var firstEncryption = SelfDES.EncryptBlock(block, k1);
            var secondDecryption = SelfDES.DecryptBlock(firstEncryption, k2);
            encryptedBlocks.Add(secondDecryption);
        }
        encryptedBlocks = EncryptWithCFB(k3, iv, encryptedBlocks, SelfDES.EncryptBlock);
        var text = ByteListToString(encryptedBlocks);

        return $"{Convert.ToBase64String(iv)}{IVConnectionString}{text}";
    }
    public static string DecryptCFB(string input, byte[] keys)
    {
        if (keys.Length != 24)
        {
            throw new ArgumentException("Key length must be 24 bytes");
        }
        var (iv, cypherText) = SplitIV(input);
        var (k1, k2, k3) = SplitKey(keys);

        var blocks = StringToByteList(cypherText);

        var intermediateBlocks = DecryptWithCFB(k3, iv, blocks, SelfDES.EncryptBlock);

        var finalDecryptedBlocks = new List<byte[]>();
        foreach (var block in intermediateBlocks)
        {
            var secondEncryption = SelfDES.EncryptBlock(block, k2);
            var firstDecryption = SelfDES.DecryptBlock(secondEncryption, k1);
            finalDecryptedBlocks.Add(firstDecryption);
        }

        finalDecryptedBlocks = RemovePaddingFromList(finalDecryptedBlocks);
        return ArrayListToString(finalDecryptedBlocks);
    }
}

class SelfTripleDesCryptography : ICryptography
{
    public string key { get; set; }
    public string GenerateKey()
    {
        return Convert.ToBase64String(Utils.GenerateTripleDesKey());
    }
    public async Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
    {
        return mode switch
        {
            BlockCypherMode.CBC =>
                SelfTripleDesOverhead.EncryptCBC(text,
                                                 Convert.FromBase64String(key),
                                                 Utils.GenerateKey(24)),
            BlockCypherMode.CFB =>
                SelfTripleDesOverhead.EncryptCFB(text,
                                                 Convert.FromBase64String(key),
                                                 Utils.GenerateKey(24)),
            _ => SelfTripleDesOverhead.Encrypt(text,
                                               Convert.FromBase64String(key))
        };
    }
    public async Task<string> Decrypt(string encryptedMessage, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
    {
        return mode switch
        {
            BlockCypherMode.CBC =>
                SelfTripleDesOverhead.DecryptCBC(encryptedMessage,
                                                 Convert.FromBase64String(key)),
            BlockCypherMode.CFB =>
                SelfTripleDesOverhead.DecryptCFB(encryptedMessage,
                                                 Convert.FromBase64String(key)),
            _ => SelfTripleDesOverhead.Decrypt(encryptedMessage,
                                               Convert.FromBase64String(key))
        };
    }
}