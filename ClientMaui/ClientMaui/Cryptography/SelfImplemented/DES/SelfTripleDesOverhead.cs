using System.Text;

namespace ClientMaui.Cryptography.SelfImplemented.DES;

class SelfTripleDesOverhead : Utils
{
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
        return ArrayListToHex(encryptedBlocks);
    }
    public static string Decrypt(string input, byte[] keys)
    {
        if (keys.Length != 24)
        {
            throw new ArgumentException("Key length must be 24 bytes");
        }
        var (k1, k2, k3) = SplitKey(keys);
        var inputBytes = Convert.FromBase64String(input);
        var blocks = SplitStringToBlocks(inputBytes);
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
        return ArrayListToHex(encryptedBlocks);
    }
    public static string DecryptCBC(string input, byte[] keys, byte[] iv)
    {
        if (keys.Length != 24)
        {
            throw new ArgumentException("Key length must be 24 bytes");
        }
        var (k1, k2, k3) = SplitKey(keys);
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes);
        var decryptedBlocks = new List<byte[]>();
        foreach (var block in blocks)
        {
            var firstDecryption = SelfDES.DecryptBlock(block, k1);
            var secondEncryption = SelfDES.EncryptBlock(firstDecryption, k2);
            decryptedBlocks.Add(secondEncryption);
        }
        decryptedBlocks = DecryptWithCBC(k3, iv, decryptedBlocks, SelfDES.DecryptBlock);
        decryptedBlocks = RemovePaddingFromList(decryptedBlocks);
        return ArrayListToHex(decryptedBlocks);
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
        return ArrayListToHex(encryptedBlocks);
    }
    public static string DecryptCFB(string input, byte[] keys, byte[] iv)
    {
        if (keys.Length != 24)
        {
            throw new ArgumentException("Key length must be 24 bytes");
        }
        var (k1, k2, k3) = SplitKey(keys);
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes);
        var decryptedBlocks = new List<byte[]>();
        foreach (var block in blocks)
        {
            var firstDecryption = SelfDES.DecryptBlock(block, k1);
            var secondEncryption = SelfDES.EncryptBlock(firstDecryption, k2);
            decryptedBlocks.Add(secondEncryption);
        }
        decryptedBlocks = DecryptWithCFB(k3, iv, decryptedBlocks, SelfDES.DecryptBlock);
        decryptedBlocks = RemovePaddingFromList(decryptedBlocks);
        return ArrayListToHex(decryptedBlocks);
    }
}