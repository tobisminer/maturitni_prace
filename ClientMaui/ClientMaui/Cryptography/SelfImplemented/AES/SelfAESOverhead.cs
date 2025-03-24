using ClientMaui.Cryptography.SelfImplemented.DES;
using System.Text;

using ClientMaui.Entities.Room;

namespace ClientMaui.Cryptography.SelfImplemented;

class SelfAESOverhead : Utils
{
    private static string IVConnectionString = "IV@CYPHERTEXT";

    public static (byte[] IV, string cyptherText) SplitIV(string text)
    {
        var split = text.Split(IVConnectionString);
        var ivBytes = Convert.FromBase64String(split[0]);
        return (ivBytes, split[1]);
    }

    public static string Encrypt(string input, byte[] key)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var blocks = SplitStringToBlocks(inputBytes, 16);
        var encryptedBlocks = blocks.Select(block => SelfAES.EncryptBlock(block, key)).ToList();
        return ByteListToString(encryptedBlocks, 16);
    }
    public static string Decrypt(string input, byte[] key)
    {
        var blocks = StringToByteList(input);
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

        var text = ByteListToString(encryptedBlocks);
        return $"{Convert.ToBase64String(iv)}{IVConnectionString}{text}";
    }

    public static string DecryptCBC(string input, byte[] key)
    {
        var (iv, cypherText) = SplitIV(input);
        var blocks = StringToByteList(cypherText);

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

        var text = ByteListToString(encryptedBlocks);
        return $"{Convert.ToBase64String(iv)}{IVConnectionString}{text}";
    }

    public static string DecryptCFB(string input, byte[] key)
    {
        var (iv, cypherText) = SplitIV(input);
        var blocks = StringToByteList(cypherText);

        var decryptedBlocks =
            DecryptWithCFB(key, iv, blocks, SelfAES.EncryptBlock);
        decryptedBlocks = RemovePaddingFromList(decryptedBlocks);
        return ArrayListToString(decryptedBlocks);
    }



}
class SelfAesCryptography : ICryptography
{
    public string key { get; set; }
    public string GenerateKey()
    {
        return Convert.ToBase64String(Utils.GenerateTripleDesKey());
    }
    public Task<string> Encrypt(string text, BlockCypherMode mode = BlockCypherMode.None)
    {
        return Task.FromResult(mode switch
        {
            BlockCypherMode.CBC =>
                SelfAESOverhead.EncryptCBC(text,
                                           Convert.FromBase64String(key),
                                           Utils.GenerateKey(24)),
            BlockCypherMode.CFB =>
                SelfAESOverhead.EncryptCFB(text,
                                           Convert.FromBase64String(key),
                                           Utils.GenerateKey(24)),
            _ => SelfAESOverhead.Encrypt(text,
                                         Convert.FromBase64String(key))
        });
    }
    public Task<string> Decrypt(string encryptedMessage, BlockCypherMode mode = BlockCypherMode.None, bool isIncoming = false)
    {
        return Task.FromResult(mode switch
        {
            BlockCypherMode.CBC =>
                SelfAESOverhead.DecryptCBC(encryptedMessage,
                                           Convert.FromBase64String(key)),
            BlockCypherMode.CFB =>
                SelfAESOverhead.DecryptCFB(encryptedMessage,
                                           Convert.FromBase64String(key)),
            _ => SelfAESOverhead.Decrypt(encryptedMessage,
                                         Convert.FromBase64String(key))
        });
    }
}