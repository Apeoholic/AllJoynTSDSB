using AdapterLib.Devices;
using AdapterLib.Protocols;
using BridgeRT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace AdapterLib.Adapters
{
    internal class TellstickNetDevice : AdapterDevice
    {
        Timer Timer;
        AdapterMethod addDeviceMethod;
        AdapterMethod getJsonMethod;
        AdapterMethod setJsonMethod;
        public AdapterSignal commandRecievedSignal { get; set; }
        DataReader DataReadObject;
        DataWriter DataWriteObject;
        public delegate void DeviceAddedEventHandler(string name,string description,string code,string protocol,string type);
        public event DeviceAddedEventHandler DeviceAdded;

        public delegate void CommandRecievedEventHandler(string command);
        public event CommandRecievedEventHandler CommandRecieved;

        private async Task ReadAsync()
        {
            Task<UInt32> loadAsyncTask;

            DataWriteObject.WriteString("1");
            await DataWriteObject.StoreAsync();
            List<byte> bytes=new List<byte>();
            await DataReadObject.LoadAsync(1);
            while (DataReadObject.UnconsumedBufferLength > 0)
            {
                var b = DataReadObject.ReadByte();
                if (b == 13)
                {
                    break;
                }
                else
                {
                    bytes.Add(b);
                }
                await DataReadObject.LoadAsync(1);
            }
            string m = System.Text.ASCIIEncoding.ASCII.GetString(bytes.ToArray());
            Message message = MessageDecoder.DecodeMessage(m);

    

            var p = GetProtocol(message.Protocol);
            if (p != null)
            {
                string dd = p.DecodeData(message.Data, message.Model);
                Debug.WriteLine(dd);
                CommandRecieved?.Invoke(dd);
            }
            else
            {
                Debug.WriteLine(JsonConvert.SerializeObject(message));
            }

            ReadAsync();
        }

        

        public string ComPort { get; set; }
        public SerialDevice SerialDevice { get; set; }
        public TellstickNetDevice(string Name,
            string VendorName,
            string Model,
            TypeEnum Type,
            string SerialNumber,
            string Description,string comPort)
            :base(Name,
             VendorName,
             Model,
             Type,
             SerialNumber,
             Description)
        {
            ComPort = comPort;


            addDeviceMethod = new AdapterMethod("Add device", "Add a device", 0);
            addDeviceMethod.InputParams.Add(new AdapterValue("Name", ""));
            addDeviceMethod.InputParams.Add(new AdapterValue("Description", ""));
            addDeviceMethod.InputParams.Add(new AdapterValue("Protocol", ""));
            addDeviceMethod.InputParams.Add(new AdapterValue("Code", ""));
            addDeviceMethod.InputParams.Add(new AdapterValue("Type ", ""));
            addDeviceMethod.InvokeAction = addDevice;
            Methods.Add(addDeviceMethod);


            getJsonMethod = new AdapterMethod("Get Json", "Get the json configuration", 0);
            getJsonMethod.OutputParams.Add(new AdapterValue("Json", ""));
            getJsonMethod.InvokeAction = async () =>
            {
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                var devicesFile = await storageFolder.CreateFileAsync("devices.json", CreationCollisionOption.OpenIfExists);

                string text = await FileIO.ReadTextAsync(devicesFile);
                getJsonMethod.OutputParams[0].Data = text;
            };
            Methods.Add(getJsonMethod);

            setJsonMethod = new AdapterMethod("Set Json", "Set the json configuration", 0);
            setJsonMethod.InputParams.Add(new AdapterValue("Json", ""));
            setJsonMethod.InvokeAction = async () =>
            {
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                var devicesFile = await storageFolder.CreateFileAsync("devices.json", CreationCollisionOption.OpenIfExists);

                await FileIO.WriteTextAsync(devicesFile,(string)setJsonMethod.InputParams[0].Data);
                
            };
            Methods.Add(setJsonMethod);

            commandRecievedSignal = new AdapterSignal("CommandRecieved");
            commandRecievedSignal.Params.Add(new AdapterValue("Command",""));
            Signals.Add(commandRecievedSignal);
            
           
        }

        //private void CreateSignals()
        //{
        //    // change of value signal
        //    AdapterSignal changeOfAttributeValue = new AdapterSignal(Constants.CHANGE_OF_VALUE_SIGNAL);
        //    //changeOfAttributeValue.AddParam(Constants.COV__PROPERTY_HANDLE);
        //    //changeOfAttributeValue.AddParam(Constants.COV__ATTRIBUTE_HANDLE);
        //    Signals.Add(changeOfAttributeValue);
        //}

        private void addDevice()
        {
            string name = addDeviceMethod.InputParams[0].Data as string;
            string description = addDeviceMethod.InputParams[1].Data as string;
            string protocol = addDeviceMethod.InputParams[2].Data as string;
            string code = addDeviceMethod.InputParams[3].Data as string;
            string type = addDeviceMethod.InputParams[4].Data as string;


            DeviceAdded?.Invoke(name, description, code, protocol, type);
        }

        public async void Initialize()
        {
            try
            {
                string _serialSelector = SerialDevice.GetDeviceSelector(ComPort);
                DeviceInformationCollection tempInfo = await DeviceInformation.FindAllAsync(_serialSelector);
                SerialDevice = await SerialDevice.FromIdAsync(tempInfo[0].Id);
                SerialDevice.BaudRate = 9600;
                SerialDevice.Parity = SerialParity.None;
                SerialDevice.StopBits = SerialStopBitCount.One;
                SerialDevice.DataBits = 8;


                DataWriteObject = new DataWriter(SerialDevice.OutputStream);
                //Set UTF8
                DataReadObject = new DataReader(SerialDevice.InputStream);
                //Set UTF8
                ReadAsync();
            }
            catch { }
        }


     

        public async Task SendCommand(string command)
        {
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(command);
            var testbytes = command.Select(c => (byte)(c & 0xff)).ToArray();


            DataWriteObject.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            DataWriteObject.WriteBytes(testbytes);
            
            await DataWriteObject.StoreAsync().AsTask();

        }

        private IBuffer getBufferFromByteArray(byte[] package)
        {
            using (DataWriter dw = new DataWriter())
            {
                dw.WriteBytes(package);
                return dw.DetachBuffer();
            }
        }


        public Protocol GetProtocol(string protocolName)
        {
            switch (protocolName.ToLower())
            {
                case "arctech":
                    return new ArctechProtocol();
                case "risingsun":
                    return new RisingSunProtocol();
                case "everflourish":
                    return new EverflourishProtocol();
                case "fineoffset":
                    return new FineoffsetProtocol();
                default:
                    return null;
            }
        }

    }
}
