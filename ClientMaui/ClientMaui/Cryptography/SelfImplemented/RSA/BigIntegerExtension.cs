using System.Numerics;

static class BigIntegerExtension
{
    // https://stackoverflow.com/questions/3432412/calculate-square-root-of-a-biginteger-system-numerics-biginteger
    public static BigInteger NewtonPlusSqrt(this BigInteger x)
    {
        if (x < 144838757784765629)    // 1.448e17 = ~1<<57
        {
            uint vInt = (uint)Math.Sqrt((ulong)x);
            if ((x >= 4503599761588224) && ((ulong)vInt * vInt > (ulong)x))  // 4.5e15 =  ~1<<52
            {
                vInt--;
            }
            return vInt;
        }

        var xAsDub = (double)x;
        switch (xAsDub)
        {
            //  long.max*long.max
            case < 8.5e37:
                {
                    var vInt = (ulong)Math.Sqrt(xAsDub);
                    BigInteger v = (vInt + ((ulong)(x / vInt))) >> 1;
                    return (v * v <= x) ? v : v - 1;
                }
            case < 4.3322e127:
                {
                    var v = (BigInteger)Math.Sqrt(xAsDub);
                    v = (v + (x / v)) >> 1;
                    if (xAsDub > 2e63)
                    {
                        v = (v + (x / v)) >> 1;
                    }
                    return (v * v <= x) ? v : v - 1;
                }
        }

        //int xLen = (int)x.GetBitLength();
        var xLen = GetBitLengthFallback(x);
        var wantedPrecision = (xLen + 1) / 2;
        var xLenMod = xLen + (xLen & 1) + 1;

        //////// Do the first Sqrt on hardware ////////
        var tempX = (long)(x >> (xLenMod - 63));
        var tempSqrt1 = Math.Sqrt(tempX);
        var valLong = (ulong)BitConverter.DoubleToInt64Bits(tempSqrt1) & 0x1fffffffffffffL;
        if (valLong == 0)
        {
            valLong = 1UL << 53;
        }

        ////////  Classic Newton Iterations ////////
        var val = ((BigInteger)valLong << 52) + ((x >> (xLenMod - (3 * 53))) / valLong);
        var size = 106;
        for (; size < 256; size <<= 1)
        {
            val = (val << (size - 1)) + ((x >> (xLenMod - (3 * size))) / val);
        }

        if (xAsDub > 4e254) // 4e254 = 1<<845.76973610139
        {
            var numOfNewtonSteps = BitOperations.Log2((uint)(wantedPrecision / size)) + 2;

            //////  Apply Starting Size  ////////
            var wantedSize = (wantedPrecision >> numOfNewtonSteps) + 2;
            var needToShiftBy = size - wantedSize;
            val >>= needToShiftBy;
            size = wantedSize;
            do
            {
                ////////  Newton Plus Iterations  ////////
                var shiftX = xLenMod - (3 * size);
                var valSqrd = (val * val) << (size - 1);
                var valSU = (x >> shiftX) - valSqrd;
                val = (val << size) + (valSU / val);
                size *= 2;
            } while (size < wantedPrecision);
        }

        /////// There are a few extra digits, let's save them ///////
        var oversidedBy = size - wantedPrecision;
        var saveDroppedDigitsBI = val & ((BigInteger.One << oversidedBy) - 1);
        var downby = (oversidedBy < 64) ? (oversidedBy >> 2) + 1 : (oversidedBy - 32);
        var saveDroppedDigits = (ulong)(saveDroppedDigitsBI >> downby);


        ////////  Shrink result to wanted Precision  ////////
        val >>= oversidedBy;


        ////////  Detect a round-up ////////
        if ((saveDroppedDigits == 0) && (val * val > x))
        {
            val--;
        }

        return val;
    }

    public static int GetBitLengthFallback(BigInteger x) // only support x > 0
    {
        var bytes = x.ToByteArray();
        var msb = bytes[^1];
        var msbBits = 0;
        while (msb != 0)
        {
            msb >>= 1;
            msbBits++;
        }
        return ((bytes.Length - 1) * 8) + msbBits;
    }
}