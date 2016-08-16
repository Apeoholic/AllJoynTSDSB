using AdapterLib.Adapters;
using AdapterLib.Protocols;
using BridgeRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLib.Devices
{
    class OutletDevice : AdapterDevice
    {
        private TellstickDevice tellStickDevice { get; set; }
        public OutletDevice(TellstickDevice tellStickDevice, string name, string vendorName, string model, TypeEnum type, string serialNumber, string description) : base(name,  vendorName, model, type, serialNumber, description)
        {
            //this.tellStickDevice = tellStickDevice;
            //base.LightingServiceHandler= new TellstickLightingServiceHandler(tellStickDevice,this);
        }
    }
}
