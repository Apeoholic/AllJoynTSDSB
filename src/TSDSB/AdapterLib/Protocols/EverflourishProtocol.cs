using System;
using System.Text;

namespace AdapterLib.Protocols
{
    class EverflourishProtocol : Protocol
    {
        public override string DecodeData(string data, string model)
        {
            var d = HexToLong(data);
            int house = 0;
            int unit = 0;
            int method = 0;

            house = (int)(d & 0xFFFC00);
            house >>= 10;

            unit = (int)(d & 0x300);
            unit >>= 8;
            unit++;  // unit from 1 to 4

            method = (int)(d & 0xF);

            if (house > 16383 || unit < 1 || unit > 4)
            {
                // not everflourish
                return "";
            }

            string retString="";
            retString += "class:command;protocol:everflourish;model:selflearning;house:" + house + ";unit:" + unit + ";method:";
            if (method == 0)
            {
                retString += "turnoff;";
            }
            else if (method == 15)
            {
                retString += "turnon;";
            }
            else if (method == 10)
            {
                retString += "learn;";
            }
            else
            {
                // not everflourish
                return "";
            }

            return retString;
        }


        // The calculation used in this function is provided by Frank Stevenson
        int calculateChecksum(int x)
        {
            var bits = new int[]
            {
                0xf, 0xa, 0x7, 0xe,
                0xf, 0xd, 0x9, 0x1,
                0x1, 0x2, 0x4, 0x8,
                0x3, 0x6, 0xc, 0xb
            };
            int bit = 1;
            int res = 0x5;
            int i;
            int lo, hi;

            if ((x & 0x3) == 3)
            {
                lo = x & 0x00ff;
                hi = x & 0xff00;
                lo += 4;
                if (lo > 0x100)
                {
                    lo = 0x12;
                }
                x = lo | hi;
            }

            for (i = 0; i < 16; i++)
            {
                if ((x & bit)>0)
                {
                    res = res ^ bits[i];
                }
                bit = bit << 1;
            }

            return res;
        }

        public override string GetString(string id, TypeEnum type, MethodEnum method, char data)
        {
            var ids = id.Split(',');
            int deviceCode = Convert.ToInt32(ids[0]);
            int code = Convert.ToInt32(ids[1])-1;
            int action =0;
            if (method ==MethodEnum.TURNON)
            {
                action = 15;
            }
            else if (method == MethodEnum.TURNOFF)
            {
                action = 0;
            }
            else if (method == MethodEnum.LEARN)
            {
                action = 10;
            }
            else
            {
                return "";
            }

            int ssss = 85;
            int sssl = 84;  // 0
            int slss = 69;  // 1

            int[] bits = { sssl, slss };
            int i, check;

            StringBuilder strCode=new StringBuilder();

            deviceCode = (deviceCode << 2) | code;

            check = calculateChecksum(deviceCode);

            char[] preamble = { 'R', (char)5, 'T', (char)114, (char)60, (char)1, (char)1, (char)105, (char)ssss, (char)ssss/*, (char)0*/ };
            strCode.Append(preamble);

            for (i = 15; i >= 0; i--)
            {
                strCode.Append((char)bits[(deviceCode >> i) & 0x01]); //1
            }
            for (i = 3; i >= 0; i--)
            {
                strCode.Append((char)bits[(check >> i) & 0x01]); //1
            }
            for (i = 3; i >= 0; i--)
            {
                strCode.Append((char)bits[(action >> i) & 0x01]); //1
            }

            strCode.Append((char)ssss); //1
            strCode.Append('+'); //1

            return strCode.ToString();
        }
    }
}
