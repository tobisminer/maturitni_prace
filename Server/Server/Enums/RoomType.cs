namespace Server.Enums
{
    // ReSharper disable InconsistentNaming
    public enum RoomType
    {
        NoEncryption,
        
        AES,
        DES,
        RSA
    }

    public static class RoomTypesExtensions
    {
        public static string ToFriendlyString(this RoomType me)
        {
            return me switch
            {
                RoomType.NoEncryption => "No Encryption",
                RoomType.AES => "AES",
                RoomType.DES => "DES",
                RoomType.RSA => "RSA",
                _ => "Unknown"
            };
        }
        public static RoomType FromFriendlyString(string friendlyString)
        {
            return friendlyString switch
            {
                "No Encryption" => RoomType.NoEncryption,
                "AES" => RoomType.AES,
                "DES" => RoomType.DES,
                "RSA" => RoomType.RSA,
                _ => RoomType.NoEncryption
            };
        }

        public static List<string> GetAll()
        {
            var roomTypes = Enum.GetValues(typeof(RoomType));
            return (from RoomType roomType in roomTypes
                select roomType.ToFriendlyString()).ToList();
        }

    }
}
