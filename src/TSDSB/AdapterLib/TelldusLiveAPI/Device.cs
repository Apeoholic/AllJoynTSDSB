using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelldusLiveAPI
{
    internal class Device
    {
        public string id { get; set; }
        public string clientDeviceId { get; set; }
        public string name { get; set; }
        public int state { get; set; }
        public string statevalue { get; set; }
        public int methods { get; set; }
        public string type { get; set; }
        public string client { get; set; }
        public string clientName { get; set; }
        public string online { get; set; }
        public int editable { get; set; }
        public int ignored { get; set; }
    }

    internal class Devices
    {
        public List<Device> device { get; set; }
    }
}
