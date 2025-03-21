﻿namespace ClientMaui.Cryptography.SelfImplemented.DES
{
    internal class SelfDES
    {
        private static readonly int[] IP =
        [
            58, 50, 42, 34, 26, 18, 10, 2,
        60, 52, 44, 36, 28, 20, 12, 4,
        62, 54, 46, 38, 30, 22, 14, 6,
        64, 56, 48, 40, 32, 24, 16, 8,
        57, 49, 41, 33, 25, 17,  9, 1,
        59, 51, 43, 35, 27, 19, 11, 3,
        61, 53, 45, 37, 29, 21, 13, 5,
        63, 55, 47, 39, 31, 23, 15, 7
        ];

        // Konečná permutace (FP = IP^-1)
        private static readonly int[] FP =
        [
            40, 8, 48, 16, 56, 24, 64, 32,
        39, 7, 47, 15, 55, 23, 63, 31,
        38, 6, 46, 14, 54, 22, 62, 30,
        37, 5, 45, 13, 53, 21, 61, 29,
        36, 4, 44, 12, 52, 20, 60, 28,
        35, 3, 43, 11, 51, 19, 59, 27,
        34, 2, 42, 10, 50, 18, 58, 26,
        33, 1, 41, 9, 49, 17, 57, 25
        ];

        // Expanzní tabulka – rozšiřuje 32bitový blok na 48 bitů
        private static readonly int[] E =
        [
            32, 1, 2, 3, 4, 5,
        4, 5, 6, 7, 8, 9,
        8, 9, 10, 11, 12, 13,
        12, 13, 14, 15, 16, 17,
        16, 17, 18, 19, 20, 21,
        20, 21, 22, 23, 24, 25,
        24, 25, 26, 27, 28, 29,
        28, 29, 30, 31, 32, 1
        ];

        // Permutační tabulka P – výsledná permutace funkce F
        private static readonly int[] P =
        [
            16, 7, 20, 21,
        29, 12, 28, 17,
        1, 15, 23, 26,
        5, 18, 31, 10,
        2, 8, 24, 14,
        32, 27, 3, 9,
        19, 13, 30, 6,
        22, 11, 4, 25
        ];

        // Permutační tabulky pro generování subklíčů:
        // PC-1: Přemění 64bitový klíč na 56bitový (odstraní paritní bity)
        private static readonly int[] PC1 =
        [
            57, 49, 41, 33, 25, 17, 9,
         1, 58, 50, 42, 34, 26, 18,
         10, 2, 59, 51, 43, 35, 27,
         19, 11, 3, 60, 52, 44, 36,
         63, 55, 47, 39, 31, 23, 15,
         7, 62, 54, 46, 38, 30, 22,
         14, 6, 61, 53, 45, 37, 29,
         21, 13, 5, 28, 20, 12, 4
        ];

        // PC-2: Přemění 56bitový klíč na 48bitový
        private static readonly int[] PC2 =
        [
            14, 17, 11, 24, 1, 5,
         3, 28, 15, 6, 21, 10,
         23, 19, 12, 4, 26, 8,
         16, 7, 27, 20, 13, 2,
         41, 52, 31, 37, 47, 55,
         30, 40, 51, 45, 33, 48,
         44, 49, 39, 56, 34, 53,
         46, 42, 50, 36, 29, 32
        ];

        // Harmonogram levotočivých posunů (16 kol)
        private static readonly int[] Shifts = new int[]
        {
        1, 1, 2, 2, 2, 2, 2, 2,
        1, 2, 2, 2, 2, 2, 2, 1
        };

        // S-boxy – 8 tabulek, každá o rozměru 4x16
        private static readonly int[][,] SBoxes = new int[][,] {
        new int[,] {
            {14,4,13,1,2,15,11,8,3,10,6,12,5,9,0,7},
            {0,15,7,4,14,2,13,1,10,6,12,11,9,5,3,8},
            {4,1,14,8,13,6,2,11,15,12,9,7,3,10,5,0},
            {15,12,8,2,4,9,1,7,5,11,3,14,10,0,6,13}
        },
        new int[,] {
            {15,1,8,14,6,11,3,4,9,7,2,13,12,0,5,10},
            {3,13,4,7,15,2,8,14,12,0,1,10,6,9,11,5},
            {0,14,7,11,10,4,13,1,5,8,12,6,9,3,2,15},
            {13,8,10,1,3,15,4,2,11,6,7,12,0,5,14,9}
        },
        new int[,] {
            {10,0,9,14,6,3,15,5,1,13,12,7,11,4,2,8},
            {13,7,0,9,3,4,6,10,2,8,5,14,12,11,15,1},
            {13,6,4,9,8,15,3,0,11,1,2,12,5,10,14,7},
            {1,10,13,0,6,9,8,7,4,15,14,3,11,5,2,12}
        },
        new int[,] {
            {7,13,14,3,0,6,9,10,1,2,8,5,11,12,4,15},
            {13,8,11,5,6,15,0,3,4,7,2,12,1,10,14,9},
            {10,6,9,0,12,11,7,13,15,1,3,14,5,2,8,4},
            {3,15,0,6,10,1,13,8,9,4,5,11,12,7,2,14}
        },
        new int[,] {
            {2,12,4,1,7,10,11,6,8,5,3,15,13,0,14,9},
            {14,11,2,12,4,7,13,1,5,0,15,10,3,9,8,6},
            {4,2,1,11,10,13,7,8,15,9,12,5,6,3,0,14},
            {11,8,12,7,1,14,2,13,6,15,0,9,10,4,5,3}
        },
        new int[,] {
            {12,1,10,15,9,2,6,8,0,13,3,4,14,7,5,11},
            {10,15,4,2,7,12,9,5,6,1,13,14,0,11,3,8},
            {9,14,15,5,2,8,12,3,7,0,4,10,1,13,11,6},
            {4,3,2,12,9,5,15,10,11,14,1,7,6,0,8,13}
        },
        new int[,] {
            {4,11,2,14,15,0,8,13,3,12,9,7,5,10,6,1},
            {13,0,11,7,4,9,1,10,14,3,5,12,2,15,8,6},
            {1,4,11,13,12,3,7,14,10,15,6,8,0,5,9,2},
            {6,11,13,8,1,4,10,7,9,5,0,15,14,2,3,12}
        },
        new int[,] {
            {13,2,8,4,6,15,11,1,10,9,3,14,5,0,12,7},
            {1,15,13,8,10,3,7,4,12,5,6,11,0,14,9,2},
            {7,11,4,1,9,12,14,2,0,6,10,13,15,3,5,8},
            {2,1,14,7,4,10,8,13,15,12,9,0,3,5,6,11}
        }
    };

        // Převod bajtového pole na bitové pole (bool[]) – bit 0 odpovídá nejvýznamnějšímu bitu prvního bajtu.
        private static bool[] GetBits(byte[] input, int bitCount)
        {
            bool[] bits = new bool[bitCount];
            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i * 8) + j < bitCount)
                    {
                        bits[(i * 8) + j] = (input[i] & (1 << (7 - j))) != 0;
                    }
                }
            }
            return bits;
        }

        // Převod bitového pole (bool[]) zpět na bajtové pole.
        private static byte[] BitsToBytes(bool[] bits)
        {
            var byteCount = bits.Length / 8;
            var bytes = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (bits[(i * 8) + j])
                    {
                        bytes[i] |= (byte)(1 << (7 - j));
                    }
                }
            }
            return bytes;
        }

        static bool[] Permutate(bool[] input, int[] permuteTable)
        {
            var output = new bool[permuteTable.Length];

            for (var i = 0; i < output.Length; i++)
            {
                output[i] = input[permuteTable[i] - 1];
            }

            return output;
        }

        static bool[] LeftMove(bool[] input, int count)
        {
            var output = new bool[input.Length];
            var bitsToEnd = new bool[count];

            for (var i = 0; i < count; i++)
            {
                bitsToEnd[i] = input[i];
            }

            for (var i = 0; i < input.Length - count; i++)
            {
                output[i] = input[i + count];
            }

            for (var i = 0; i < count; i++)
            {
                output[output.Length - i - 1] = bitsToEnd[i];
            }
            return output;
        }

        static List<bool[]> Generate16SubKeys(bool[] main64bitKey)
        {
            var reducedKey = Permutate(main64bitKey, PC1);
            var output = new List<bool[]>();
            var C = new bool[28];
            var D = new bool[28];
            Array.Copy(reducedKey, 0, C, 0, 28);
            Array.Copy(reducedKey, 28, D, 0, 28);

            for (var i = 0; i < 16; i++)
            {
                C = LeftMove(C, Shifts[i]);
                D = LeftMove(D, Shifts[i]);
                var combined = new bool[56];
                Array.Copy(C, 0, combined, 0, 28);
                Array.Copy(D, 0, combined, 28, 28);
                output.Add(Permutate(combined, PC2));
            }

            return output;
        }

        static bool[] F(bool[] dataBlock, bool[] key)
        {
            var extendedBlock = Permutate(dataBlock, E);
            var xorResult = new bool[48];
            for (int i = 0; i < 48; i++)
            {
                xorResult[i] = extendedBlock[i] ^ key[i];
            }
            var sBoxOutput = new bool[32];
            for (var i = 0; i < 8; i++)
            {
                var start = i * 6;
                var row = (xorResult[start] ? 2 : 0) + (xorResult[start + 5] ? 1 : 0);
                var col = 0;
                for (var j = 1; j <= 4; j++)
                {
                    col = (col << 1) + (xorResult[start + j] ? 1 : 0);
                }
                var sValue = SBoxes[i][row, col];

                for (var j = 0; j < 4; j++)
                {
                    sBoxOutput[(i * 4) + j] = ((sValue >> (3 - j)) & 1) == 1;
                }
            }

            // Konečná permutace pomocí tabulky P
            bool[] fResult = Permutate(sBoxOutput, P);
            return fResult;
        }

        public static byte[] EncryptBlock(byte[] plainTextBlock, byte[] key)
        {
            var blockBits = GetBits(plainTextBlock, 64);
            var ipBlockBits = Permutate(blockBits, IP);
            var L = new bool[32];
            var R = new bool[32];
            Array.Copy(ipBlockBits, 0, L, 0, 32);
            Array.Copy(ipBlockBits, 32, R, 0, 32);
            var keyBit = GetBits(key, 64);
            var subKeys = Generate16SubKeys(keyBit);

            for (int roundNumber = 0; roundNumber < 16; roundNumber++)
            {
                var previousR = R;
                var fResult = F(R, subKeys[roundNumber]);
                var newR = new bool[32];

                for (int i = 0; i < 32; i++)
                {
                    newR[i] = L[i] ^ fResult[i];
                }

                L = previousR;
                R = newR;
            }

            var notMixedOutput = new bool[64];
            Array.Copy(R, 0, notMixedOutput, 0, 32);
            Array.Copy(L, 0, notMixedOutput, 32, 32);

            return BitsToBytes(Permutate(notMixedOutput, FP));

        }
        public static byte[] DecryptBlock(byte[] cipherTextBlock, byte[] key)
        {
            var cipherBits = GetBits(cipherTextBlock, 64);
            var ipBits = Permutate(cipherBits, IP);

            var L = new bool[32];
            var R = new bool[32];
            Array.Copy(ipBits, 0, L, 0, 32);
            Array.Copy(ipBits, 32, R, 0, 32);

            var keyBits = GetBits(key, 64);
            var subKeys = Generate16SubKeys(keyBits);

            for (var round = 15; round >= 0; round--)
            {
                var previousR = R;
                var fOutput = F(R, subKeys[round]);
                var newR = new bool[32];
                for (int i = 0; i < 32; i++)
                {
                    newR[i] = L[i] ^ fOutput[i];
                }
                L = previousR;
                R = newR;
            }

            var notMixedOutput = new bool[64];
            Array.Copy(R, 0, notMixedOutput, 0, 32);
            Array.Copy(L, 0, notMixedOutput, 32, 32);

            var plainBits = Permutate(notMixedOutput, FP);
            return BitsToBytes(plainBits);
        }
    }
}
