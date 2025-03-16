using System.Text;

namespace ClientMaui.Cryptography.SelfImplemented.DES;

class SelfDesOverhead : DesUtils
{
    public static string Encrypt(string input, byte[] key)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes);
        var encryptedBlocks = blocks.Select(block => SelfDES.EncryptBlock(block, key)).ToList();
        return ArrayListToHex(encryptedBlocks);
    }

    public static string Decrypt(string input, byte[] key)
    {

        var inputBytes = Convert.FromBase64String(input);
        var blocks = SplitStringToBlocks(RemovePadding(inputBytes));
        var decryptedBlocks = blocks.Select(block => SelfDES.DecryptBlock(block, key)).ToList();

        return ArrayListToString(decryptedBlocks);
    }

    public static string EncryptCBC(string input, byte[] key, byte[] iv)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes);

        var encryptedBlocks =
            EncryptWithCBC(key, iv, blocks, SelfDES.EncryptBlock);
        return ArrayListToHex(encryptedBlocks);
    }

    public static string DecryptCBC(string input, byte[] key, byte[] iv)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(RemovePadding(inputBytes));

        var decryptedBlocks =
            DecryptWithCBC(key, iv, blocks, SelfDES.DecryptBlock);
        return ArrayListToString(decryptedBlocks);
    }
    public static string EncryptCFB(string input, byte[] key, byte[] iv)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes);

        var encryptedBlocks =
            EncryptWithCFB(key, iv, blocks, SelfDES.EncryptBlock);
        return ArrayListToHex(encryptedBlocks);
    }

    public static string DecryptCFB(string input, byte[] key, byte[] iv)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(RemovePadding(inputBytes));

        var decryptedBlocks =
            DecryptWithCFB(key, iv, blocks, SelfDES.DecryptBlock);
        return ArrayListToString(decryptedBlocks);
    }

}