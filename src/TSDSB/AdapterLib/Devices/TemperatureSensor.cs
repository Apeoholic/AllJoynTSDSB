using org.alljoyn.SmartSpaces.Environment;
using org.alljoyn.SmartSpaces.Environment;
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
    class TemperatureSensor : ICurrentTemperature, INotifyPropertyChanged
    {
        Timer timer = null;
        public string ID;
        public TemperatureSensor(string id)
        {
            ID = id;
            timer = new Timer(new TimerCallback(async (obj) => { await Update(); }), null, 5000, 15 * 60 * 1000);
        }

        public async Task Update()
        {
            var client = new TelldusLiveAPIClient();
            var sensor = await client.GetSensor(ID);

            var temp = sensor.data.FirstOrDefault(s => s.name == "temp");
            if (temp != null)
            {
                currentTemperature = Convert.ToDouble(temp.value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ICurrentTemperature.CurrentValue"));
            }
        }

        short ICurrentTemperature.Version
        {
            get
            {
                return 1;
            }
        }

        private double currentTemperature = double.NaN;
        double ICurrentTemperature.CurrentValue
        {
            get
            {
                return currentTemperature;
            }
        }

        double ICurrentTemperature.Precision
        {
            get
            {
                return 0.1d;
            }
        }

        ushort ICurrentTemperature.UpdateMinTime
        {
            get
            {
                return 15000;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
    }
}
