using AdapterLib.Helpers;
using System;
using System.Diagnostics;

namespace AdapterLib.Protocols
{
    class FineoffsetProtocol : Protocol
    {
        public override string GetString(string id, TypeEnum type, MethodEnum method, char data)
        {
            throw new NotImplementedException();
        }

        public override string DecodeData(string data, string model)
        {
            try
            {

                var bytes = data.StringToByteArray();
                var humidity = bytes[3];
                var temperature = (double)(((bytes[1] & 0x07) << 8) | bytes[2])/10.0;

                
                long type = bytes[0] >>4;
                long id = (bytes[0] << 4) | (bytes[0] >> 4);

                string retString;
                retString = "class:sensor;protocol:fineoffset;id:" + id + ";model:";

                if (humidity <= 100)
                {
                    retString += "temperaturehumidity;humidity:" + humidity + ";";
                }
                else if (humidity == 0xFF)
                {
                    retString += "temperature;";
                }
                else
                {
                    return "";
                }

                retString += "temp:" + temperature + ";";

                return retString;

            }
            catch
            { }
            return "";
        }
    }
}