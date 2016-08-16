using System;
using System.Collections;
using System.Linq;

namespace AdapterLib.Helpers
{
    static class ProtocolHelpers
    {
        public static BitArray ToBitArray(this long numeral)
        {
            var bytes = BitConverter.GetBytes(numeral);
            return new BitArray(bytes);
        }


        public static byte[] StringToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static long ToLong(this BitArray bitArray,int start=0,int length = 0)
        {
            long value = 0;
            long multiplier = 0;

            if (length == 0)
                length = bitArray.Length;
            else
                length = length + start;
            

            for (int i = start; i < length; i++)
            {
                if (bitArray[i])
                    value += Convert.ToInt16(Math.Pow(2, multiplier));

                multiplier++;
            }

            return value;
        }

    }
}
