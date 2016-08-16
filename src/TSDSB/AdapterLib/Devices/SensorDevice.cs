using AdapterLib.Adapters;
using AdapterLib.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLib.Devices
{
    class SensorDevice: AdapterDevice
    {

        private TellstickDevice tellStickDevice { get; set; }
        //public SensorDevice(TellstickDevice tellStickDevice, string name, string vendorName, string model, TypeEnum type, string serialNumber, string description) : base(name,  vendorName, model, type, serialNumber, description)
        //{
        //    this.tellStickDevice = tellStickDevice;
            
        //}
    }
}
