using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using BridgeRT;
using AdapterLib.Devices;
using Windows.Devices.AllJoyn;
using AdapterLib.Connectors;
using System.Threading;
using Windows.UI.Xaml;
using AdapterLib.Helpers;
using TelldusLiveAPI;

namespace AdapterLib
{
    public sealed class Adapter : IAdapter
    {
        private const uint ERROR_SUCCESS = 0;
        private const uint ERROR_INVALID_HANDLE = 6;

        // Device Arrival and Device Removal Signal Indices
        private const int DEVICE_ARRIVAL_SIGNAL_INDEX = 0;
        private const int DEVICE_ARRIVAL_SIGNAL_PARAM_INDEX = 0;
        private const int DEVICE_REMOVAL_SIGNAL_INDEX = 1;
        private const int DEVICE_REMOVAL_SIGNAL_PARAM_INDEX = 0;

        public string Vendor { get; }

        public string AdapterName { get; }

        public string Version { get; }

        public string ExposedAdapterPrefix { get; }

        public string ExposedApplicationName { get; }

        public Guid ExposedApplicationGuid { get; }

        public IList<IAdapterSignal> Signals { get; }

        
        public Adapter()
        {
            Windows.ApplicationModel.Package package = Windows.ApplicationModel.Package.Current;
            Windows.ApplicationModel.PackageId packageId = package.Id;
            Windows.ApplicationModel.PackageVersion versionFromPkg = packageId.Version;

            this.Vendor = "Apeoholic";
            this.AdapterName = "TS DSB";

            // the adapter prefix must be something like "com.mycompany" (only alpha num and dots)
            // it is used by the Device System Bridge as root string for all services and interfaces it exposes
            this.ExposedAdapterPrefix = "com." + this.Vendor.ToLower();
            this.ExposedApplicationGuid = Guid.Parse("{0xbab9fab6,0x2e33,0x4c88,{0x95,0x20,0xcd,0x3c,0xc4,0xd3,0x0b,0x9d}}");

            if (null != package && null != packageId)
            {
                this.ExposedApplicationName = packageId.Name;
                this.Version = versionFromPkg.Major.ToString() + "." +
                               versionFromPkg.Minor.ToString() + "." +
                               versionFromPkg.Revision.ToString() + "." +
                               versionFromPkg.Build.ToString();
            }
            else
            {
                this.ExposedApplicationName = "TSDeviceSystemBridge";
                this.Version = "0.0.0.0";
            }

            try
            {
                this.Signals = new List<IAdapterSignal>();
                this.devices = new List<IAdapterDevice>();
                this.signalListeners = new Dictionary<int, IList<SIGNAL_LISTENER_ENTRY>>();

                //Create Adapter Signals
                this.createSignals();
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }
        
        public uint SetConfiguration([ReadOnlyArray] byte[] ConfigurationData)
        {
            return ERROR_SUCCESS;
        }

        public uint GetConfiguration(out byte[] ConfigurationDataPtr)
        {
            ConfigurationDataPtr = null;

            return ERROR_SUCCESS;
        }
        AdapterDevice[] types;
        ConnectorBase Connector;
        //VolumeProducer producer;
        //AllJoynBusAttachment busAttachment;
        public uint Initialize()
        {
            var tellduslivedevice = new AdapterDevice("Telldus Live","Telldus","","","","TelldusLive","Telldus Live connection",this);
          
            var getTokenURL = new AdapterMethod("Step_1_GetTokenURL", "This will return a URL, open the url in a web brower and follow the instructions", 0);
            getTokenURL.OutputParams.Add(new AdapterValue("Url",""));
            tellduslivedevice.Methods.Add(getTokenURL);


            var finish = new AdapterMethod("Step_2_Confirm", "This method might timeout, but don't worry about.", 0);
            
            tellduslivedevice.Methods.Add(finish);

            tellduslivedevice.Icon = new AdapterIcon("ms-appx:///AdapterLib/Assets/TelldusLive.png");
            this.devices.Add(tellduslivedevice);
            Connector = new TelldusLiveConnector();
            start();
            
            
            return ERROR_SUCCESS;
        }

        private void start()
        {
            
            Connector.DeviceArrived += Connector_DeviceArrived;
            Connector.DeviceRemoved += Connector_DeviceRemoved;
            Connector.Start(this);
        }

    private void Connector_DeviceRemoved(object sender, AdapterDevice e)
    {
        this.devices.Remove(e);
        NotifyDeviceRemoval(e);
    }

    private void Connector_DeviceArrived(object sender, AdapterDevice e)
    {
        this.devices.Add(e);
        NotifyDeviceArrival(e);
    }


    public uint Shutdown()
        {
            return ERROR_SUCCESS;
        }

        public uint EnumDevices(
            ENUM_DEVICES_OPTIONS Options,
            out IList<IAdapterDevice> DeviceListPtr,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            try
            {
                DeviceListPtr = new List<IAdapterDevice>(this.devices);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }

            return ERROR_SUCCESS;
        }

        public uint GetProperty(
            IAdapterProperty Property,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        public uint SetProperty(
            IAdapterProperty Property,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        public uint GetPropertyValue(
            IAdapterProperty Property,
            string AttributeName,
            out IAdapterValue ValuePtr,
            out IAdapterIoRequest RequestPtr)
        {
            ValuePtr = null;
            RequestPtr = null;

            // find corresponding attribute
            foreach (var attribute in ((AdapterProperty)Property).Attributes)
            {
                if (attribute.Value.Name == AttributeName)
                {
                    ValuePtr = attribute.Value;
                    return ERROR_SUCCESS;
                }
            }

            return ERROR_INVALID_HANDLE;
        }

        public uint SetPropertyValue(
            IAdapterProperty Property,
            IAdapterValue Value,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            // find corresponding attribute
            foreach (var attribute in ((AdapterProperty)Property).Attributes)
            {
                if (attribute.Value.Name == Value.Name)
                {
                    attribute.Value.Data = Value.Data;
                    return ERROR_SUCCESS;
                }
            }

            return ERROR_INVALID_HANDLE;
        }


        //private void SaveConfiguration(string publicKey, string privateKey, string token, string tokenSecret)
        //{
        //    TelldusLiveAPIClient client = new TelldusLiveAPIClient();
        //    client.SaveConfiguration(publicKey, privateKey, token, tokenSecret);

        //    start();
        //}
        private string GetTokenURL()
        {
            TelldusLiveAPIClient client = new TelldusLiveAPIClient();
            return client.GetURL();

        }


        private void Finish()
        {
            TelldusLiveAPIClient client = new TelldusLiveAPIClient();
            client.Finish();

        }
        public uint CallMethod(
            IAdapterMethod Method,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;
            switch (Method.Name)
            {
               
                case "Step_1_GetTokenURL":
                    Method.OutputParams[0].Data= GetTokenURL();
                    break;
                case "Step_2_Confirm":
                    Finish();
                    start();
                    break;


            }

            //if (Method is AdapterMethod)
            //{
            //    var am = ((AdapterMethod)Method);
            //    am.MethodInfo.Invoke(am.Device, Method.InputParams.Select(p => p.Data).ToArray());
            //}

                    return ERROR_SUCCESS;
        }

        public uint RegisterSignalListener(
            IAdapterSignal Signal,
            IAdapterSignalListener Listener,
            object ListenerContext)
        {
            if (Signal == null || Listener == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            int signalHashCode = Signal.GetHashCode();

            SIGNAL_LISTENER_ENTRY newEntry;
            newEntry.Signal = Signal;
            newEntry.Listener = Listener;
            newEntry.Context = ListenerContext;

            lock (this.signalListeners)
            {
                if (this.signalListeners.ContainsKey(signalHashCode))
                {
                    this.signalListeners[signalHashCode].Add(newEntry);
                }
                else
                {
                    IList<SIGNAL_LISTENER_ENTRY> newEntryList;

                    try
                    {
                        newEntryList = new List<SIGNAL_LISTENER_ENTRY>();
                    }
                    catch (OutOfMemoryException ex)
                    {
                        throw;
                    }

                    newEntryList.Add(newEntry);
                    this.signalListeners.Add(signalHashCode, newEntryList);
                }
            }

            return ERROR_SUCCESS;
        }

        public uint UnregisterSignalListener(
            IAdapterSignal Signal,
            IAdapterSignalListener Listener)
        {
            return ERROR_SUCCESS;
        }

        public uint NotifySignalListener(IAdapterSignal Signal)
        {
            if (Signal == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            int signalHashCode = Signal.GetHashCode();

            lock (this.signalListeners)
            {
                IList<SIGNAL_LISTENER_ENTRY> listenerList = this.signalListeners[signalHashCode];
                foreach (SIGNAL_LISTENER_ENTRY entry in listenerList)
                {
                    IAdapterSignalListener listener = entry.Listener;
                    object listenerContext = entry.Context;
                    listener.AdapterSignalHandler(Signal, listenerContext);
                }
            }

            return ERROR_SUCCESS;
        }

        public uint NotifyDeviceArrival(IAdapterDevice Device)
        {
            if (Device == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            IAdapterSignal deviceArrivalSignal = this.Signals[DEVICE_ARRIVAL_SIGNAL_INDEX];
            IAdapterValue signalParam = deviceArrivalSignal.Params[DEVICE_ARRIVAL_SIGNAL_PARAM_INDEX];
            signalParam.Data = Device;
            this.NotifySignalListener(deviceArrivalSignal);

            return ERROR_SUCCESS;
        }

        internal void SignalChangeOfAttributeValue(IAdapterDevice device, IAdapterProperty property, IAdapterAttribute attribute)
        {
            // find change of value signal of that end point (end point == bridgeRT device)

            var covSignal = device.Signals.OfType<AdapterSignal>().FirstOrDefault(s => s.Name == Constants.CHANGE_OF_VALUE_SIGNAL);
            if (covSignal == null)
            {
                // no change of value signal
                return;
            }

            // set property and attribute param of COV signal
            // note that 
            // - ZCL cluster correspond to BridgeRT property 
            // - ZCL attribute correspond to BridgeRT attribute 
            var param = covSignal.Params.FirstOrDefault(p => p.Name == Constants.COV__PROPERTY_HANDLE);
            if (param == null)
            {
                // signal doesn't have the expected parameter
                return;
            }
            param.Data = property;

            param = covSignal.Params.FirstOrDefault(p => p.Name == Constants.COV__ATTRIBUTE_HANDLE);
            if (param == null)
            {
                // signal doesn't have the expected parameter
                return;
            }
            param.Data = attribute.Value;

            // signal change of value to BridgeRT
            NotifySignalListener(covSignal);
        }


        public uint NotifyDeviceRemoval(IAdapterDevice Device)
        {
            if (Device == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            IAdapterSignal deviceRemovalSignal = this.Signals[DEVICE_REMOVAL_SIGNAL_INDEX];
            IAdapterValue signalParam = deviceRemovalSignal.Params[DEVICE_REMOVAL_SIGNAL_PARAM_INDEX];
            signalParam.Data = Device;
            this.NotifySignalListener(deviceRemovalSignal);

            return ERROR_SUCCESS;
        }

        private void createSignals()
        {
            try
            {
                // Device Arrival Signal
                AdapterSignal deviceArrivalSignal = new AdapterSignal(Constants.DEVICE_ARRIVAL_SIGNAL);
                AdapterValue deviceHandle_arrival = new AdapterValue(
                                                            Constants.DEVICE_ARRIVAL__DEVICE_HANDLE,
                                                            null);
                deviceArrivalSignal.Params.Add(deviceHandle_arrival);

                // Device Removal Signal
                AdapterSignal deviceRemovalSignal = new AdapterSignal(Constants.DEVICE_REMOVAL_SIGNAL);
                AdapterValue deviceHandle_removal = new AdapterValue(
                                                            Constants.DEVICE_REMOVAL__DEVICE_HANDLE,
                                                            null);
                deviceRemovalSignal.Params.Add(deviceHandle_removal);

                // Add Signals to the Adapter Signals
                this.Signals.Add(deviceArrivalSignal);
                this.Signals.Add(deviceRemovalSignal);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        private struct SIGNAL_LISTENER_ENTRY
        {
            // The signal object
            internal IAdapterSignal Signal;

            // The listener object
            internal IAdapterSignalListener Listener;

            //
            // The listener context that will be
            // passed to the signal handler
            //
            internal object Context;
        }

        // List of Devices
        private IList<IAdapterDevice> devices;

        // A map of signal handle (object's hash code) and related listener entry
        private Dictionary<int, IList<SIGNAL_LISTENER_ENTRY>> signalListeners;
    }
}
