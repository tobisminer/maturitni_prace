using System.Security.Cryptography;

namespace ClientMaui.Entities.Room
{
    public enum BlockCypherMode
    {
        ECB,
        CBC,
        CFB,
        None
    }
    public class BlockCypherModeHelper
    {
        public static string ConvertToString(BlockCypherMode blockCypherMode)
        {
            return blockCypherMode switch
            {
                BlockCypherMode.ECB => "ECB",
                BlockCypherMode.CBC => "CBC",
                BlockCypherMode.CFB => "CFB",
                _ => "Neznamy"
            };
        }
        public static BlockCypherMode ConvertFromString(string? blockCypherMode)
        {
            return blockCypherMode switch
            {
                "ECB" => BlockCypherMode.ECB,
                "CBC" => BlockCypherMode.CBC,
                "CFB" => BlockCypherMode.CFB,
                _ => BlockCypherMode.None
            };
        }

        public static CipherMode ConvertToCipherMode(
            BlockCypherMode blockCypherMode)
        {
            return blockCypherMode switch
            {
                BlockCypherMode.ECB => CipherMode.ECB,
                BlockCypherMode.CBC => CipherMode.CBC,
                BlockCypherMode.CFB => CipherMode.CFB,
                _ => CipherMode.ECB
            };
        }
    }
}
