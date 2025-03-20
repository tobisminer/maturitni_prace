using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using HealthKit;

namespace ClientMaui.Cryptography.SelfImplemented.RSA
{
    internal class SelfRSA
    {
        public static List<int> splitNumberToSmallerPowers(int number)
        {
            var result = new List<int>();
            var remainder = number;

            while (remainder > 0)
            {
                var mathLog = Math.Log(remainder, 2);
                var power = (int)Math.Floor(mathLog);
                var value = (int)Math.Pow(2, power);
                result.Add(value);
                remainder -= value;
            }
            return result;

        }
        public static int modPow(int number, int power, int mod)
        {
            var powers = splitNumberToSmallerPowers(power);
            var lookupTable = new Dictionary<int, int>();
            if (powers.Contains(1))
            {
                lookupTable.Add(1, number % mod);
            }

            for (var i = 2; i <= powers.Max(); i*=2)
            {
                
                lookupTable.Add(i, (int)Math.Pow(lookupTable[i/2], 2) % mod);
                
            }
            var result = powers.Aggregate(BigInteger.One, (current, item) => current * lookupTable[item]);

            return (int)(result % mod);
        }
        static bool IsPrime(int number)
        {
            if (number < 2)
                return false;
            var stop = Math.Sqrt(number);
            for (var i = 2; i <= stop; i++)
            {
                if (number % i == 0)
                    return false;
            }
            return true;
        }

        static int GenerateRandomPrime(BigInteger min, BigInteger max)
        {
        }

    }
}
