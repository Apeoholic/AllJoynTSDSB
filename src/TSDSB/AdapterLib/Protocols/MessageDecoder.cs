using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AdapterLib.Protocols
{
    class MessageDecoder
    {
        public static Message DecodeMessage(string message)
        {
            Message m = new Message();
            var parts = message.Replace("\n", "").Substring(2).Split(';');
            foreach (var part in parts)
            {
                var values = part.Split(':');
                switch (values[0])
                {
                    case "class":
                        m.Class = values[1];
                        break;
                    case "protocol":
                        m.Protocol = values[1];
                        break;
                    case "model":
                        m.Model = values[1];
                        break;
                    case "method":
                        m.Method = values[1];
                        break;
                    case "data":
                        m.Data = values[1];
                        break;
                }
            }
            return m;
        }
    }
}
