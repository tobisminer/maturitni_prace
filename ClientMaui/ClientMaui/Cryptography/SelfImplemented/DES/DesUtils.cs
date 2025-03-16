using System.Security.Cryptography;
using System.Text;

namespace ClientMaui.Cryptography.SelfImplemented.DES;

class DesUtils
{
    public static List<byte[]> SplitStringToBlocks(byte[] inputBytes, int blockSize = 8)
    {
        var output = new List<byte[]>();
        var totalBytes = inputBytes.Length;
        var copiedBytes = 0;

        while (copiedBytes < totalBytes)
        {
            var remainingBytes = totalBytes - copiedBytes;
            var block = new byte[blockSize];

            if (remainingBytes >= blockSize)
            {
                Array.Copy(inputBytes, copiedBytes, block, 0, blockSize);
                copiedBytes += blockSize;
            }
            else
            {
                // Kopírování zbývajících bajtů
                Array.Copy(inputBytes, copiedBytes, block, 0, remainingBytes);
                copiedBytes += remainingBytes;

                // Padding podle PKCS7
                var padValue = (byte)(blockSize - remainingBytes);
                for (int i = remainingBytes; i < blockSize; i++)
                {
                    block[i] = padValue;
                }
            }

            output.Add(block);
        }

        return output;
    }
    public static byte[] RemovePadding(byte[] input)
    {
        var padValue = input[^1];
        var output = new byte[input.Length - padValue];
        Array.Copy(input, output, output.Length);
        return output;
    }

    public static string ArrayListToHex(ICollection<byte[]> blocks)
    {
        var finalArray = new byte[blocks.Count * 8];
        var count = 0;
        foreach (var block in blocks)
        {
            Array.Copy(block, 0, finalArray, count, 8);
            count += 8;
        }
        return Convert.ToBase64String(finalArray);
    }

    public static string ArrayListToString(ICollection<byte[]> blocks)
    {
        return blocks.Aggregate("", (current, block) => current + Encoding.UTF8.GetString(block));
    }

    public static byte[] GenerateKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var key = new byte[8];
        rng.GetBytes(key);
        return key;
    }
    public static byte[] GenerateTripleDesKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var key = new byte[24];
        rng.GetBytes(key);
        return key;
    }
    public static byte[] GenerateIV()
    {
        using var rng = RandomNumberGenerator.Create();
        var iv = new byte[8];
        rng.GetBytes(iv);
        return iv;
    }

    public static byte[] XOR(byte[] block, byte[] iv)
    {
        var output = new byte[block.Length];
        for (var i = 0; i < block.Length; i++)
        {
            output[i] = (byte)(block[i] ^ iv[i % block.Length]);
        }
        return output;
    }

    public static List<byte[]> EncryptWithCBC(byte[] key,
        byte[] iv,
        List<byte[]> blocks,
        Delegate encryptFunc)
    {
        var xorBlock = iv;
        var encryptedBlocks = new List<byte[]>();
        foreach (var block in blocks)
        {
            var xoredBlock = XOR(block, xorBlock);
            var encryptedBlock = encryptFunc.DynamicInvoke(xoredBlock, key) as byte[];
            encryptedBlocks.Add(encryptedBlock);
            xorBlock = encryptedBlock;
        }
        return encryptedBlocks;
    }
    public static List<byte[]> DecryptWithCBC(byte[] key,
        byte[] iv,
        List<byte[]> blocks,
        Delegate decryptFunc)
    {
        var xorBlock = iv;
        var decryptedBlocks = new List<byte[]>();
        foreach (var block in blocks)
        {
            var decryptedBlock = decryptFunc.DynamicInvoke(block, key) as byte[];
            var xoredBlock = XOR(decryptedBlock, xorBlock);
            decryptedBlocks.Add(xoredBlock);
            xorBlock = block;
        }
        return decryptedBlocks;
    }
    public static List<byte[]> EncryptWithCFB(byte[] key,
        byte[] iv,
        List<byte[]> blocks,
        Delegate encryptFunc)
    {
        var inputBlock = iv;
        var encryptedBlocks = new List<byte[]>();
        foreach (var block in blocks)
        {
            var encryptedBlock = encryptFunc.DynamicInvoke(inputBlock, key) as byte[];
            var xoredBlock = XOR(encryptedBlock, block);
            encryptedBlocks.Add(xoredBlock);
            inputBlock = xoredBlock;
        }
        return encryptedBlocks;
    }
    public static List<byte[]> DecryptWithCFB(byte[] key,
        byte[] iv,
        List<byte[]> blocks,
        Delegate decryptFunc)
    {
        var inputBlock = iv;
        var decryptedBlocks = new List<byte[]>();
        foreach (var block in blocks)
        {
            var decryptedBlock = decryptFunc.DynamicInvoke(inputBlock, key) as byte[];
            var xoredBlock = XOR(decryptedBlock, block);
            decryptedBlocks.Add(block);
            inputBlock = xoredBlock;
        }
        return decryptedBlocks;
    }
    public static (byte[], byte[], byte[]) SplitKey(byte[] key)
    {
        if (key.Length != 24)
        {
            throw new ArgumentException("Key length must be 24 bytes");
        }
        var k1 = new byte[8];
        var k2 = new byte[8];
        var k3 = new byte[8];
        Array.Copy(key, 0, k1, 0, 8);
        Array.Copy(key, 8, k2, 0, 8);
        Array.Copy(key, 16, k3, 0, 8);
        return (k1, k2, k3);
    }


}