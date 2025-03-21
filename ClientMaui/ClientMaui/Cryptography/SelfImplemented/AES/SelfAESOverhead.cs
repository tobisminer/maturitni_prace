using System.Text;

namespace ClientMaui.Cryptography.SelfImplemented;

class SelfAESOverhead : Utils
{
    public static string Encrypt(string input, byte[] key)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes, 16);
        var encryptedBlocks = blocks.Select(block => SelfAES.EncryptBlock(block, key)).ToList();
        return ArrayListToHex(encryptedBlocks, 16);
    }
    public static string Decrypt(string input, byte[] key)
    {
        var inputBytes = Convert.FromBase64String(input);
        var blocks = SplitStringToBlocks(inputBytes, 16);
        var decryptedBlocks = blocks.Select(block => SelfAES.DecryptBlock(block, key)).ToList();
        decryptedBlocks = RemovePaddingFromList(decryptedBlocks);
        return ArrayListToString(decryptedBlocks);
    }
    public static string EncryptCBC(string input, byte[] key, byte[] iv)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes, 16);

        var encryptedBlocks =
            EncryptWithCBC(key, iv, blocks, SelfAES.EncryptBlock);
        return ArrayListToHex(encryptedBlocks);
    }

    public static string DecryptCBC(string input, byte[] key, byte[] iv)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes, 16);

        var decryptedBlocks =
            DecryptWithCBC(key, iv, blocks, SelfAES.DecryptBlock);
        decryptedBlocks = RemovePaddingFromList(decryptedBlocks);
        return ArrayListToString(decryptedBlocks);
    }
    public static string EncryptCFB(string input, byte[] key, byte[] iv)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes, 16);

        var encryptedBlocks =
            EncryptWithCFB(key, iv, blocks, SelfAES.EncryptBlock);
        return ArrayListToHex(encryptedBlocks);
    }

    public static string DecryptCFB(string input, byte[] key, byte[] iv)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes, 16);

        var decryptedBlocks =
            DecryptWithCFB(key, iv, blocks, SelfAES.DecryptBlock);
        decryptedBlocks = RemovePaddingFromList(decryptedBlocks);
        return ArrayListToString(decryptedBlocks);
    }



}