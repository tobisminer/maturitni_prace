using System.Numerics;
using System.Security.Cryptography;


namespace ClientMaui.Cryptography.SelfImplemented.RSA
{
    internal class SelfRSA
    {
        static List<BigInteger> splitNumberToSmallerPowers(BigInteger number)
        {
            var result = new List<BigInteger>();
            var remainder = number;

            while (remainder > 0)
            {
                var mathLog = BigInteger.Log(remainder, 2);
                var power = (int)Math.Floor(mathLog);
                var value = BigInteger.Pow(2, power);
                result.Add(value);
                remainder -= value;
            }
            return result;

        }
        static BigInteger modPow(BigInteger number, BigInteger power, BigInteger mod)
        {
            var powers = splitNumberToSmallerPowers(power);
            var lookupTable = new Dictionary<BigInteger, BigInteger>();
            if (powers.Contains(1))
            {
                lookupTable.Add(1, number % mod);
            }

            for (var i = new BigInteger(2); i <= powers.Max(); i *= 2)
            {

                lookupTable.Add(i, BigInteger.Pow(lookupTable[i / 2], 2) % mod);

            }
            var result = powers.Aggregate(BigInteger.One, (current, item) => current * lookupTable[item]);

            return result % mod;
        }
        static BigInteger GenerateRandomBigInteger(int bitLength)
        {
            int byteLength = (bitLength + 7) / 8;
            byte[] bytes = new byte[byteLength];
            RandomNumberGenerator.Fill(bytes);

            // Zajistíme, že nejvyšší bit je nastaven, aby číslo mělo správnou bitovou délku
            bytes[^1] |= 0x80;

            return new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        }

        // Miller-Rabin test na prvočíselnost
        static bool IsPrime(BigInteger n, int k = 10)
        {
            if (n < 2) return false;
            if (n == 2 || n == 3) return true;
            if (n % 2 == 0) return false;

            // Rozložíme n-1 na d * 2^r
            BigInteger d = n - 1;
            int r = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }

            // Opakujeme test k-krát
            for (int i = 0; i < k; i++)
            {
                BigInteger a = (GenerateRandomBigInteger(n.ToByteArray().Length * 8) % (n - 3)) + 2;
                BigInteger x = BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1) continue;

                bool composite = true;
                for (int j = 0; j < r - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == n - 1)
                    {
                        composite = false;
                        break;
                    }
                }
                if (composite) return false;
            }
            return true;
        }

        // Generování prvočísla o dané bitové délce
        static BigInteger GeneratePrime(int bitLength)
        {
            BigInteger prime;
            do
            {
                prime = GenerateRandomBigInteger(bitLength) | 1; // Ujistíme se, že číslo je liché
            } while (!IsPrime(prime));
            return prime;
        }

        //Rozšířený Euklidův algoritmus
        static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m, t, q;
            BigInteger x0 = 0, x1 = 1;

            if (m == 1) return 0;

            while (a > 1)
            {
                q = a / m;
                t = m;

                m = a % m;
                a = t;
                t = x0;

                x0 = x1 - (q * x0);
                x1 = t;
            }

            if (x1 < 0)
                x1 += m0;

            return x1;
        }
        static BigInteger gcd(BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                (b, a) = (a % b, b);
            }
            return a;
        }

        public static (BigInteger E, BigInteger N, BigInteger D) GenerateRSAKeyPair(int keySize = 1024)
        {
            GC.TryStartNoGCRegion(keySize * 1000 * 1000, keySize);
            var size = keySize / 2;
            var P = GeneratePrime(size);
            var Q = GeneratePrime(size);
            while (P == Q)
            {
                Q = GeneratePrime(size);
            }
            var N = P * Q;
            var phi = (P - 1) * (Q - 1);
            var E = new BigInteger(65537);
            var D = new BigInteger(0);
            while (true)
            {
                if (gcd(E, phi) == 1)
                {
                    D = ModInverse(E, phi);
                    break;
                }
                E++;
            }
            GC.EndNoGCRegion();
            return (E, N, D);
        }
        static BigInteger EncryptCharacter(int message, BigInteger E, BigInteger N)
        {
            return modPow(message, E, N);
        }
        static BigInteger DecryptCharacter(BigInteger message, BigInteger D, BigInteger N)
        {
            return modPow(message, D, N);
        }
        public static List<BigInteger> Encrypt(string message, BigInteger E, BigInteger N)
        {
            var result = message.Select(character => EncryptCharacter(character, E, N)).ToList();
            return result;
        }
        public static string Decrypt(List<BigInteger> message, BigInteger D, BigInteger N)
        {
            var result = message.Select(character => DecryptCharacter(character, D, N)).ToList();

            var bytes = result.Select(bi => (char)(int)bi).ToArray();
            return new string(bytes);
        }




    }
}