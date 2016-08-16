using AdapterLib.Protocols;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelldusLiveAPI;

namespace AdapterLib.Devices
{
    class Device: INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Protocol { get; set; }
        public TypeEnum DeviceType { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        private TelldusLiveLightingServiceHandler lsfHandler;

        Timer timer = null;
        public string ID;

        public event PropertyChangedEventHandler PropertyChanged;

        public Device(string id, TelldusLiveLightingServiceHandler lsfHandler)
        {
            ID = id;
            this.lsfHandler = lsfHandler;
            timer = new Timer(new TimerCallback(async (obj) => { await Update(); }), null, 5000, 5 * 60 * 1000);
        }

        public async Task Update()
        {
            var client = new TelldusLiveAPIClient();
            var device = await client.GetDevice(ID);

            lsfHandler.SetState(device.state);
        }
    }
}
