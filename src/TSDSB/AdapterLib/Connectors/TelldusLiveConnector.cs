using AdapterLib.Devices;
using BridgeRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelldusLiveAPI;

namespace AdapterLib.Connectors
{
    internal class TelldusLiveConnector:ConnectorBase
    {
        TelldusLiveAPI.TelldusLiveAPIClient client = new TelldusLiveAPIClient();
        Adapter Adapter = null;
        public override async Task Start(Adapter adapter)
        {
            Adapter = adapter;
            client.LoadConfiguration();
            if (!string.IsNullOrEmpty(client.Token))
            {
                var devices = await client.GetDevices();
                foreach (var device in devices.Where(d => d.type == "device"))
                {
                    //Convert to a device
                    var d = new AdapterDevice(device.name, "", "", device.methods.ToString(), "", device.id, "", adapter);
                    var lsfHandler = new TelldusLiveLightingServiceHandler(client, d);
                    d.Device = new Devices.Device(device.id, lsfHandler);
                    d.LightingServiceHandler = lsfHandler;
                    if (device.state == 1)
                    {
                        lsfHandler.SetState(device.state);
                    }
                    lsfHandler.PropertyChanged += LsfHandler_PropertyChanged;
                    d.Icon = new AdapterIcon("ms-appx:///AdapterLib/Assets/bulb.png");
                    OnDeviceArrived(d);
                }

                var sensors = await client.GetSensors();
                foreach (var sensor in sensors.Where(s => s.model == "temperature" || s.model == "temperaturehumidity"))
                {
                    //Convert to a device
                    var s = new AdapterDevice(sensor.name, "", "", "", "", sensor.id, "", adapter);

                    if (sensor.model == "temperature")
                        s.Device = new TemperatureSensor(sensor.id);
                    else
                        s.Device = new TemperatureHumiditySensor(sensor.id);
                    s.Icon = new AdapterIcon("ms-appx:///AdapterLib/Assets/temperature.png");
                    s.Initialize();
                    OnDeviceArrived(s);
                }
            }
        }

        private void LsfHandler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var lsf = sender as TelldusLiveLightingServiceHandler;
            if (lsf != null)
            {
                var signal = lsf.LampState_LampStateChanged;
                signal.Params.Add(new AdapterValue(Constants.SIGNAL_PARAMETER__LAMP_ID__NAME, lsf.LampDetails_LampID));
                signal.Params.Add(new AdapterValue("OnOff", lsf.LampState_OnOff));
                Adapter.NotifySignalListener(signal);
            }
        }

        private void Currentdevice_DeviceStateChanged(object sender, int e)
        {
            
        }
    }
}
