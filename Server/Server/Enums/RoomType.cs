﻿using Server.Models;

namespace Server.Enums
{
    // ReSharper disable InconsistentNaming
    public enum RoomType
    {
        NoEncryption,
        DES,
        TRIPLE_DES,
        AES,
        ARCFOUR,
        RSA,
        RSA_AES,
        SELFDES,
        SELFTRIPLEDES,
        SELFAES,
        SELFRSA,
        SELFRSA_DES,
        SELFRSA_AES
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
                RoomType.TRIPLE_DES => "Triple DES",
                RoomType.ARCFOUR => "RC4",
                RoomType.RSA => "RSA",
                RoomType.RSA_AES => "RSA+AES",
                RoomType.SELFDES => "Self DES",
                RoomType.SELFTRIPLEDES => "Self Triple DES",
                RoomType.SELFAES => "Self AES",
                RoomType.SELFRSA => "Self RSA",
                RoomType.SELFRSA_DES => "Self RSA+DES",
                RoomType.SELFRSA_AES => "Self RSA+AES",
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
                "Triple DES" => RoomType.TRIPLE_DES,
                "RC4" => RoomType.ARCFOUR,
                "RSA" => RoomType.RSA,
                "RSA+AES" => RoomType.RSA_AES,
                "Self DES" => RoomType.SELFDES,
                "Self Triple DES" => RoomType.SELFTRIPLEDES,
                "Self AES" => RoomType.SELFAES,
                "Self RSA" => RoomType.SELFRSA,
                "Self RSA+DES" => RoomType.SELFRSA_DES,
                "Self RSA+AES" => RoomType.SELFRSA_AES,
                _ => RoomType.NoEncryption
            };
        }
        public static bool isSecure(this RoomType me)
        {
            return me switch
            {
                RoomType.NoEncryption => false,
                RoomType.AES => false,
                RoomType.DES => false,
                RoomType.TRIPLE_DES => false,
                RoomType.ARCFOUR => false,
                RoomType.RSA => true,
                RoomType.RSA_AES => true,
                RoomType.SELFDES => false,
                RoomType.SELFTRIPLEDES => false,
                RoomType.SELFAES => true,
                RoomType.SELFRSA => true,
                RoomType.SELFRSA_DES => false,
                RoomType.SELFRSA_AES => true,
                _ => false
            };
        }
        public static bool isAsymmetric(this RoomType me)
        {
            return me switch
            {
                RoomType.RSA => true,
                RoomType.RSA_AES => true,
                RoomType.SELFRSA => true,
                RoomType.SELFRSA_DES => true,
                RoomType.SELFRSA_AES => true,
                _ => false
            };
        }

        public static List<RoomTypeJson> GetAll()
        {
            var roomTypes = Enum.GetValues(typeof(RoomType));
            return (from RoomType roomType in roomTypes select new RoomTypeJson { name = roomType.ToFriendlyString(), isSecure = roomType.isSecure() }).ToList();

        }

    }

}