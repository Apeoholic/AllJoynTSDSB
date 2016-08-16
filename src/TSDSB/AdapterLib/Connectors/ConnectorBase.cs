using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLib.Connectors
{
    internal class ConnectorBase
    {
        virtual public async Task Start(Adapter adapter)
        { }

        public event EventHandler<AdapterDevice> DeviceArrived;
        protected virtual void OnDeviceArrived(AdapterDevice device)
        {
            var eh = DeviceArrived;
            if (eh != null)
            {
                eh(this, device);
            }
        }

        public event EventHandler<AdapterDevice> DeviceRemoved;
        protected virtual void OnDeviceRemoved(AdapterDevice device)
        {
            var eh = DeviceRemoved;
            if (eh != null)
            {
                eh(this, device);
            }
        }

    }
}
