using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AdapterLib.Protocols
{
    class ArctechProtocol : Protocol
    {

        int lastArctecCodeSwitchWasTurnOff = 0;

        public override string GetString(string id, TypeEnum type, MethodEnum method, char data)
        {
            if (type == TypeEnum.CodeSwitch)
            {
                return getStringCodeSwitch(id,method);
            }
            else if (type == TypeEnum.Bell)
            {
                return getStringBell(id);
            }
            if ((method == MethodEnum.TURNON) && type ==TypeEnum.SelflearningDimmer) {
                return getStringSelflearning(id,MethodEnum.DIM, (char)255);
            }
            if (method == MethodEnum.LEARN)
            {
                string str = getStringSelflearning(id,MethodEnum.TURNON, data);

                //TellStick* ts = reinterpret_cast<TellStick*>(controller);
                //if (!ts)
                //{
                //    return str;
                //}
                //if (ts->pid() == 0x0c30 && ts->firmwareVersion() <= 2)
                //{

                //    str.Insert(0, 1, 2); str.Insert(0, 1, 'R');
                //    for (int i = 0; i < 5; ++i)
                //    {
                //        controller->send(str);
                //    }
                //}
                return str;
            }
            return getStringSelflearning(id,method, data);
        }

        string getStringCodeSwitch(string id,MethodEnum method)
        {
            var ids = id.Split(',');
           string  house = ids[0];
           int code = Convert.ToInt32(ids[1])-1;

            StringBuilder strReturn = new StringBuilder("S");

            int intHouse = house[0] - 'A';
            strReturn.Append(getCodeSwitchTuple(intHouse));
            strReturn.Append(getCodeSwitchTuple(code));

            if (method == MethodEnum.TURNON)
            {
                strReturn.Append("$k$k$kk$$kk$$kk$$k+");
            }
            else if (method == MethodEnum.TURNOFF)
            {
                strReturn.Append(getOffCode());
            }
            else
            {
                return "";
            }
            return strReturn.ToString();
        }

        string getStringBell(string id)
        {
            var ids = id.Split(',');
            string house =ids[0];
            int code = Convert.ToInt32(ids[1]);
            StringBuilder strReturn = new StringBuilder("S");

            int intHouse = house[0] - 'A';
            strReturn.Append(getCodeSwitchTuple(intHouse));
            strReturn.Append("$kk$$kk$$kk$$k$k"); strReturn.Append("$kk$$kk$$kk$$kk$$k+"); return strReturn.ToString();
        }

        string getStringSelflearning(string id,MethodEnum method, /*unsigned*/ char level)
        {
               var ids = id.Split(',');
               int house = Convert.ToInt32(ids[0]);
               int code = Convert.ToInt32(ids[1])-1;
               int group = 1;
               if(ids.Length>2)
               {
                    group= Convert.ToInt32(ids[2]);
                }
            return getStringSelflearningForCode(house, code,group, method, level);
        }

        string getStringSelflearningForCode(int intHouse, int intCode,int group, MethodEnum method, /*unsigned*/ char level)
        {

            /*unsigned*/
            char[] START= new char[] { 'T', (char)127, (char)255, (char)24, (char)1 };//, '\0' };
            
            StringBuilder strMessage= new StringBuilder(new string(START));
            //strMessage.Append(1, (method == MethodEnum.DIM ? 147 : 132));
            strMessage.Append((method == MethodEnum.DIM ? (char)147 : (char)132));
            StringBuilder m=new StringBuilder();
            for (int i = 25; i >= 0; --i)
            {
                m.Append((intHouse & (1 << i))==0? "01" : "10");
            }
            //Group
            group = group + 1;
            m.Append(Convert.ToString(group,2).PadLeft(2));
            
            if (method == MethodEnum.DIM)
            {
                m.Append("00");
            }
            else if (method == MethodEnum.TURNOFF)
            {
                m.Append("01");
            }
            else if (method == MethodEnum.TURNON)
            {
                m.Append("10");
            }
            else
            {
                return "";
            }

            for (int i = 3; i >= 0; --i)
            {
                m.Append((intCode &( 1 << i))==0 ? "01" : "10");
            }

            if (method == MethodEnum.DIM)
            {
                /*unsigned*/
                char newLevel =(char) (level / 16);
                for (int i = 3; i >= 0; --i)
                {
                    m.Append((newLevel & (1 << i))==0 ? "01" : "10");
                }
            }
            //lkjlkjlkj
            m.Append("0");

            /*unsigned*/
            byte code = 9;
            for (int i = 0; i < m.Length; ++i)
            {
                code <<= 4;
                if (m[i] == '1')
                {
                    code |= 8;
                }
                else
                {
                    code |= 10;
                }
                if (i % 2 == 0)
                {

                    //strMessage.Append(1, code);
                    strMessage.Append((char)code);
                    code = 0;
                }
            }
            strMessage.Append("+");

            return strMessage.ToString();
        }

        public override string DecodeData(string data, string model)
        {
            var d = HexToLong(data);
            
            if (model == "selflearning")
            {
                return decodeDataSelfLearning(d);
            }
            else
            {
                return decodeDataCodeSwitch(d);
            }
        }

        string decodeDataSelfLearning(long allData)
        {
            /*unsigned*/
            int house = 0;
            /*unsigned*/
            int unit = 0;
            /*unsigned*/
            int group = 0;
            /*unsigned*/
            int method = 0;

            house = (int)(allData & 0xFFFFFFC0);
            house >>= 6;

            group = (int)(allData & 0x20);
            group >>= 5;

            method = (int)(allData & 0x10);
            method >>= 4;

            unit = (int)(allData & 0xF);
            unit++;

            if (house < 1 || house > 67108863 || unit < 1 || unit > 16)
            {
                return "";
            }

            string retString="";
            retString += "class:command;protocol:arctech;model:selflearning;house:" + house + ";unit:" + unit + ";group:" + group + ";method:";
            if (method == 1)
            {
                retString += "turnon;";
            }
            else if (method == 0)
            {
                retString += "turnoff;";
            }
            else
            {
                return "";
            }

            return retString;
        }

        string decodeDataCodeSwitch(long allData)
        {
            /*unsigned*/
            int house = 0;
            /*unsigned*/
            int unit = 0;
            /*unsigned*/
            int method = 0;

            method = (int)allData & 0xF00;
            method >>= 8;

            unit = (int)allData & 0xF0;
            unit >>= 4;
            unit++;

            house = (int)allData & 0xF;

            if (house > 16 || unit < 1 || unit > 16)
            {
                return "";
            }

            house = house + 'A';
            if (method != 6 && lastArctecCodeSwitchWasTurnOff == 1)
            {
                lastArctecCodeSwitchWasTurnOff = 0;
                return "";
            }

            if (method == 6)
            {
                lastArctecCodeSwitchWasTurnOff = 1;
            }

            string retString="";
            retString += "class:command;protocol:arctech;model:codeswitch;house:" + house;

            if (method == 6)
            {
                retString += ";unit:" + unit + ";method:turnoff;";
            }
            else if (method == 14)
            {
                retString += ";unit:" + unit + ";method:turnon;";
            }
            else if (method == 15)
            {
                retString += ";method:bell;";
            }
            else
            {
                return "";
            }

            return retString;
        }

        string getCodeSwitchTuple(int intCode)
        {
            StringBuilder strReturn = new StringBuilder();
            for (int i = 0; i < 4; ++i)
            {
                if ((intCode & 1)==1)
                {
                    strReturn.Append("$kk$");
                }
                else
                {
                    strReturn.Append("$k$k");
                }
                intCode >>= 1;
            }
            return strReturn.ToString();
        }

        string getOffCode()
        {
            return "$k$k$kk$$kk$$k$k$k+";
        }
    }
}
