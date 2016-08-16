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
using Windows.UI.Xaml;

namespace AdapterLib.Devices
{
    class TemperatureHumiditySensor : ICurrentTemperature, INotifyPropertyChanged, ICurrentHumidity
    {
        Timer timer = null;
        public string ID;
        public TemperatureHumiditySensor(string id)
        {
            ID = id;
            timer = new Timer(new TimerCallback(async (obj) => { await Update(); }), null, 5000, 15 * 60 * 1000);
           
        }

        public async Task Update()
        {
            var client = new TelldusLiveAPIClient();
            var sensor=await client.GetSensor(ID);

            var temp = sensor.data.FirstOrDefault(s => s.name == "temp");
            if (temp != null)
            {
                currentTemperature = Convert.ToDouble(temp.value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ICurrentTemperature.CurrentValue"));
            }

            var humidity = sensor.data.FirstOrDefault(s => s.name == "humidity");
            if (humidity != null)
            {
                currentHumidity = Convert.ToDouble(humidity.value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ICurrentHumidity.CurrentValue"));
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

        short ICurrentHumidity.Version
        {
            get
            {
                return 1;
            }
        }

        double currentHumidity = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        double ICurrentHumidity.CurrentValue
        {
            get
            {
                return currentHumidity;
            }
        }

        double ICurrentHumidity.MaxValue
        {
            get
            {
                return 100d;
            }
        }
        
    }
}
