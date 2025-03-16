﻿using ClientMaui.Cryptography.SelfImplemented.DES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientMaui.Cryptography.SelfImplemented
{
    internal class SelfAES
    {
        private static readonly byte[] sbox =
        [
            0x63,0x7c,0x77,0x7b,0xf2,0x6b,0x6f,0xc5,0x30,0x01,0x67,0x2b,0xfe,0xd7,0xab,0x76,
        0xca,0x82,0xc9,0x7d,0xfa,0x59,0x47,0xf0,0xad,0xd4,0xa2,0xaf,0x9c,0xa4,0x72,0xc0,
        0xb7,0xfd,0x93,0x26,0x36,0x3f,0xf7,0xcc,0x34,0xa5,0xe5,0xf1,0x71,0xd8,0x31,0x15,
        0x04,0xc7,0x23,0xc3,0x18,0x96,0x05,0x9a,0x07,0x12,0x80,0xe2,0xeb,0x27,0xb2,0x75,
        0x09,0x83,0x2c,0x1a,0x1b,0x6e,0x5a,0xa0,0x52,0x3b,0xd6,0xb3,0x29,0xe3,0x2f,0x84,
        0x53,0xd1,0x00,0xed,0x20,0xfc,0xb1,0x5b,0x6a,0xcb,0xbe,0x39,0x4a,0x4c,0x58,0xcf,
        0xd0,0xef,0xaa,0xfb,0x43,0x4d,0x33,0x85,0x45,0xf9,0x02,0x7f,0x50,0x3c,0x9f,0xa8,
        0x51,0xa3,0x40,0x8f,0x92,0x9d,0x38,0xf5,0xbc,0xb6,0xda,0x21,0x10,0xff,0xf3,0xd2,
        0xcd,0x0c,0x13,0xec,0x5f,0x97,0x44,0x17,0xc4,0xa7,0x7e,0x3d,0x64,0x5d,0x19,0x73,
        0x60,0x81,0x4f,0xdc,0x22,0x2a,0x90,0x88,0x46,0xee,0xb8,0x14,0xde,0x5e,0x0b,0xdb,
        0xe0,0x32,0x3a,0x0a,0x49,0x06,0x24,0x5c,0xc2,0xd3,0xac,0x62,0x91,0x95,0xe4,0x79,
        0xe7,0xc8,0x37,0x6d,0x8d,0xd5,0x4e,0xa9,0x6c,0x56,0xf4,0xea,0x65,0x7a,0xae,0x08,
        0xba,0x78,0x25,0x2e,0x1c,0xa6,0xb4,0xc6,0xe8,0xdd,0x74,0x1f,0x4b,0xbd,0x8b,0x8a,
        0x70,0x3e,0xb5,0x66,0x48,0x03,0xf6,0x0e,0x61,0x35,0x57,0xb9,0x86,0xc1,0x1d,0x9e,
        0xe1,0xf8,0x98,0x11,0x69,0xd9,0x8e,0x94,0x9b,0x1e,0x87,0xe9,0xce,0x55,0x28,0xdf,
        0x8c,0xa1,0x89,0x0d,0xbf,0xe6,0x42,0x68,0x41,0x99,0x2d,0x0f,0xb0,0x54,0xbb,0x16
        ];

        // Inverzní S-box pro AES
        private static readonly byte[] invSbox =
        [
            0x52,0x09,0x6A,0xD5,0x30,0x36,0xA5,0x38,0xBF,0x40,0xA3,0x9E,0x81,0xF3,0xD7,0xFB,
        0x7C,0xE3,0x39,0x82,0x9B,0x2F,0xFF,0x87,0x34,0x8E,0x43,0x44,0xC4,0xDE,0xE9,0xCB,
        0x54,0x7B,0x94,0x32,0xA6,0xC2,0x23,0x3D,0xEE,0x4C,0x95,0x0B,0x42,0xFA,0xC3,0x4E,
        0x08,0x2E,0xA1,0x66,0x28,0xD9,0x24,0xB2,0x76,0x5B,0xA2,0x49,0x6D,0x8B,0xD1,0x25,
        0x72,0xF8,0xF6,0x64,0x86,0x68,0x98,0x16,0xD4,0xA4,0x5C,0xCC,0x5D,0x65,0xB6,0x92,
        0x6C,0x70,0x48,0x50,0xFD,0xED,0xB9,0xDA,0x5E,0x15,0x46,0x57,0xA7,0x8D,0x9D,0x84,
        0x90,0xD8,0xAB,0x00,0x8C,0xBC,0xD3,0x0A,0xF7,0xE4,0x58,0x05,0xB8,0xB3,0x45,0x06,
        0xD0,0x2C,0x1E,0x8F,0xCA,0x3F,0x0F,0x02,0xC1,0xAF,0xBD,0x03,0x01,0x13,0x8A,0x6B,
        0x3A,0x91,0x11,0x41,0x4F,0x67,0xDC,0xEA,0x97,0xF2,0xCF,0xCE,0xF0,0xB4,0xE6,0x73,
        0x96,0xAC,0x74,0x22,0xE7,0xAD,0x35,0x85,0xE2,0xF9,0x37,0xE8,0x1C,0x75,0xDF,0x6E,
        0x47,0xF1,0x1A,0x71,0x1D,0x29,0xC5,0x89,0x6F,0xB7,0x62,0x0E,0xAA,0x18,0xBE,0x1B,
        0xFC,0x56,0x3E,0x4B,0xC6,0xD2,0x79,0x20,0x9A,0xDB,0xC0,0xFE,0x78,0xCD,0x5A,0xF4,
        0x1F,0xDD,0xA8,0x33,0x88,0x07,0xC7,0x31,0xB1,0x12,0x10,0x59,0x27,0x80,0xEC,0x5F,
        0x60,0x51,0x7F,0xA9,0x19,0xB5,0x4A,0x0D,0x2D,0xE5,0x7A,0x9F,0x93,0xC9,0x9C,0xEF,
        0xA0,0xE0,0x3B,0x4D,0xAE,0x2A,0xF5,0xB0,0xC8,0xEB,0xBB,0x3C,0x83,0x53,0x99,0x61,
        0x17,0x2B,0x04,0x7E,0xBA,0x77,0xD6,0x26,0xE1,0x69,0x14,0x63,0x55,0x21,0x0C,0x7D
        ];
        private static readonly byte[] Rcon =
        [
            0x00, // nepoužito
            0x01,0x02,0x04,0x08,0x10,0x20,0x40,0x80,0x1B,0x36
        ];
        public static byte[] KeyExpansion(byte[] key)
        {
            var wordCountForRound = 4;      
            var keyWordCount = 4;  //Počet slov v prvotním klíči   
            var roundCount = 10;
            int totalWords = wordCountForRound * (roundCount + 1);
            byte[] expandedKey = new byte[totalWords * 4];

            
            Array.Copy(key, expandedKey, key.Length);

            byte[] temp = new byte[4];

            for (int i = keyWordCount; i < totalWords; i++)
            {
                // Načteme předchozí slovo
                Array.Copy(expandedKey, (i - 1) * 4, temp, 0, 4);

                if (i % keyWordCount == 0)
                {
                    // Left shift
                    byte t = temp[0];
                    temp[0] = temp[1];
                    temp[1] = temp[2];
                    temp[2] = temp[3];
                    temp[3] = t;
                    // Dosazení pomocí S-boxu
                    temp[0] = sbox[temp[0]];
                    temp[1] = sbox[temp[1]];
                    temp[2] = sbox[temp[2]];
                    temp[3] = sbox[temp[3]];
                    // XOR s hodnotou z Rcon
                    temp[0] ^= Rcon[i / keyWordCount];
                }

                // Vytvoření nového slova jako XOR mezi slovem Nk pozic zpět a upraveným temp
                for (int j = 0; j < 4; j++)
                {
                    expandedKey[i * 4 + j] = (byte)(expandedKey[(i - keyWordCount) * 4 + j] ^ temp[j]);
                }
            }
            return expandedKey;
        }
        public static void AddRoundKey(byte[,] state, byte[] roundKey)
        {
            for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                state[i, j] ^= roundKey[i + 4 * j];
        }
        /// <summary>
        /// SubBytes – nahradí každý bajt stavu hodnotou z S-boxu
        /// </summary>
        public static void SubBytes(byte[,] state)
        {
            for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
                state[i, j] = sbox[state[i, j]];
        }
        public static void InvSubBytes(byte[,] state)
        {
            for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                state[i, j] = invSbox[state[i, j]];
        }

        public static void ShiftRows(byte[,] state)
        {
            // Řádek 0 se neposouvá
            // Řádek 1 se posune o 1
            (state[1, 0], state[1, 1], state[1, 2], state[1, 3]) = (state[1, 1],
                state[1, 2], state[1, 3], state[1, 0]);

            //Řádek 2 se posune o 2
            (state[2, 0], state[2, 1], state[2, 2], state[2, 3]) = (state[2, 2],
                state[2, 3], state[2, 0], state[2, 1]);

            //Řádek 3 se posune o 3
            (state[3, 0], state[3, 1], state[3, 2], state[3, 3]) = (state[3, 3],
                state[3, 0], state[3, 1], state[3, 2]);
        }

        public static void InvShiftRows(byte[,] state)
        {
            // Řádek 0 se neposouvá
            // Řádek 1 se posune o 1 doprava
            (state[1, 0], state[1, 1], state[1, 2], state[1, 3]) = (state[1, 3],
                state[1, 0], state[1, 1], state[1, 2]);

            //Řádek 2 se posune o 2 doprava
            (state[2, 0], state[2, 1], state[2, 2], state[2, 3]) = (state[2, 2],
                state[2, 3], state[2, 0], state[2, 1]);

            //Řádek 3 se posune o 3 doprava
            (state[3, 0], state[3, 1], state[3, 2], state[3, 3]) = (state[3, 1],
                state[3, 2], state[3, 3], state[3, 0]);
        }
        public static byte xtime(byte x)
        {
            return (byte)((x << 1) ^ (((x >> 7) & 1) * 0x1B));
        }
        /// <summary>
        /// MixColumns – míchání sloupců stavu pomocí matice
        /// Každý sloupec je transformován:
        /// s'[0] = 2·s0 ⊕ 3·s1 ⊕ s2 ⊕ s3  
        /// s'[1] = s0 ⊕ 2·s1 ⊕ 3·s2 ⊕ s3  
        /// s'[2] = s0 ⊕ s1 ⊕ 2·s2 ⊕ 3·s3  
        /// s'[3] = 3·s0 ⊕ s1 ⊕ s2 ⊕ 2·s3  
        /// </summary>
        public static void MixColumns(byte[,] state)
        {
            for (int j = 0; j < 4; j++)
            {
                var a0 = state[0, j];
                var a1 = state[1, j];
                var a2 = state[2, j];
                var a3 = state[3, j];

                var r0 = (byte)(xtime(a0) ^ (a1 ^ xtime(a1)) ^ a2 ^ a3);
                var r1 = (byte)(a0 ^ xtime(a1) ^ (a2 ^ xtime(a2)) ^ a3);
                var r2 = (byte)(a0 ^ a1 ^ xtime(a2) ^ (a3 ^ xtime(a3)));
                var r3 = (byte)((a0 ^ xtime(a0)) ^ a1 ^ a2 ^ xtime(a3));

                state[0, j] = r0;
                state[1, j] = r1;
                state[2, j] = r2;
                state[3, j] = r3;
            }
        }
        public static byte Multiply(byte a, byte b)
        {
            byte result = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((b & 1) != 0)
                    result ^= a;
                bool hiBitSet = (a & 0x80) != 0;
                a <<= 1;
                if (hiBitSet)
                    a ^= 0x1B;
                b >>= 1;
            }
            return result;
        }

        public static void InvMixColumns(byte[,] state)
        {
            for (int j = 0; j < 4; j++)
            {
                byte s0 = state[0, j];
                byte s1 = state[1, j];
                byte s2 = state[2, j];
                byte s3 = state[3, j];

                state[0, j] = (byte)(Multiply(s0, 0x0e) ^ Multiply(s1, 0x0b) ^ Multiply(s2, 0x0d) ^ Multiply(s3, 0x09));
                state[1, j] = (byte)(Multiply(s0, 0x09) ^ Multiply(s1, 0x0e) ^ Multiply(s2, 0x0b) ^ Multiply(s3, 0x0d));
                state[2, j] = (byte)(Multiply(s0, 0x0d) ^ Multiply(s1, 0x09) ^ Multiply(s2, 0x0e) ^ Multiply(s3, 0x0b));
                state[3, j] = (byte)(Multiply(s0, 0x0b) ^ Multiply(s1, 0x0d) ^ Multiply(s2, 0x09) ^ Multiply(s3, 0x0e));
            }
        }
        public static byte[] EncryptBlock(byte[] input, byte[] key)
        {
            var Nr = 10;
            var state = new byte[4, 4];
            for (int i = 0; i < 16; i++)
                state[i % 4, i / 4] = input[i];

            var expandedKey = KeyExpansion(key);
            var roundKey = new byte[16];
            Array.Copy(expandedKey, 0, roundKey, 0, 16);
            AddRoundKey(state, roundKey);

            for (var round = 1; round < Nr; round++)
            {
                SubBytes(state);
                ShiftRows(state);
                MixColumns(state);
                Array.Copy(expandedKey, round * 16, roundKey, 0, 16);
                AddRoundKey(state, roundKey);
            }

            SubBytes(state);
            ShiftRows(state);
            Array.Copy(expandedKey, Nr * 16, roundKey, 0, 16);
            AddRoundKey(state, roundKey);

            var output = new byte[16];
            for (var i = 0; i < 16; i++)
                output[i] = state[i % 4, i / 4];
            return output;
        }

        public static byte[] DecryptBlock(byte[] input, byte[] key)
        {
            var Nr = 10;
            var state = new byte[4, 4];
            for (int i = 0; i < 16; i++)
                state[i % 4, i / 4] = input[i];

            var expandedKey = KeyExpansion(key);
            var roundKey = new byte[16];
            Array.Copy(expandedKey, Nr * 16, roundKey, 0, 16);
            AddRoundKey(state, roundKey);

            for (var round = Nr - 1; round >= 1; round--)
            {
                InvShiftRows(state);
                InvSubBytes(state);
                Array.Copy(expandedKey, round * 16, roundKey, 0, 16);
                AddRoundKey(state, roundKey);
                InvMixColumns(state);
            }

            InvShiftRows(state);
            InvSubBytes(state);
            Array.Copy(expandedKey, 0, roundKey, 0, 16);
            AddRoundKey(state, roundKey);

            var output = new byte[16];
            for (var i = 0; i < 16; i++)
                output[i] = state[i % 4, i / 4];
            return output;
        }
    }
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
            var blocks = SplitStringToBlocks(RemovePadding(inputBytes), 16);
            var decryptedBlocks = blocks.Select(block => SelfAES.DecryptBlock(block, key)).ToList();

            return ArrayListToString(decryptedBlocks);
        }
        public static string EncryptCBC(string input, byte[] key, byte[] iv)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var blocks = SplitStringToBlocks(inputBytes);

            var encryptedBlocks =
                EncryptWithCBC(key, iv, blocks, SelfAES.EncryptBlock);
            return ArrayListToHex(encryptedBlocks);
        }

        public static string DecryptCBC(string input, byte[] key, byte[] iv)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var blocks = SplitStringToBlocks(RemovePadding(inputBytes));

            var decryptedBlocks =
                DecryptWithCBC(key, iv, blocks, SelfAES.DecryptBlock);
            return ArrayListToString(decryptedBlocks);
        }
        public static string EncryptCFB(string input, byte[] key, byte[] iv)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var blocks = SplitStringToBlocks(inputBytes);

            var encryptedBlocks =
                EncryptWithCFB(key, iv, blocks, SelfAES.EncryptBlock);
            return ArrayListToHex(encryptedBlocks);
        }

        public static string DecryptCFB(string input, byte[] key, byte[] iv)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var blocks = SplitStringToBlocks(RemovePadding(inputBytes));

            var decryptedBlocks =
                DecryptWithCFB(key, iv, blocks, SelfAES.DecryptBlock);
            return ArrayListToString(decryptedBlocks);
        }



    }
}
