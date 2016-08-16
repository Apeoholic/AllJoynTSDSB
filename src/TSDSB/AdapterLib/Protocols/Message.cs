using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdapterLib.Protocols
{
    class Message
    {
        public string Class { get; set; }
        public string Method { get; set; }
        public string Model { get; set; }
        public string Protocol { get; set; }
        public string Data { get; set; }

    }
}
