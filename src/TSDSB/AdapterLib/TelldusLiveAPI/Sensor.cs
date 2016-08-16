using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelldusLiveAPI
{
    internal class Datum
    {
        public string name { get; set; }
        public string value { get; set; }
        public string scale { get; set; }
    }

    internal class Sensor
    {
        public string id { get; set; }
        public string name { get; set; }
        public int lastUpdated { get; set; }
        public int ignored { get; set; }
        public string client { get; set; }
        public string clientName { get; set; }
        public string online { get; set; }
        public int editable { get; set; }
        public int battery { get; set; }
        public int keepHistory { get; set; }
        public string protocol { get; set; }
        public string model { get; set; }
        public string sensorId { get; set; }
        public List<Datum> data { get; set; }
    }

    internal class SensorResponse
    {
        public List<Sensor> sensor { get; set; }
    }
}
