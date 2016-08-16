

using AdapterLib.Protocols;
using BridgeRT;
using System;
namespace AdapterLib
{
    internal class TellstickLightingServiceHandler : ILSFHandler
    {
        TellstickDevice tellstickDevice { get; set; }
        AdapterDevice device { get; set; }

        Protocol protocol;
        public TellstickLightingServiceHandler(TellstickDevice tellstickDevice, AdapterDevice device)
        {
            this.tellstickDevice = tellstickDevice;
            this.device = device;
            protocol = tellstickDevice.GetProtocol(device.Model);
            LampDetails_Color = false;
            LampDetails_Dimmable = device.Type==TypeEnum.SelflearningDimmer;
            LampDetails_HasEffects = false;
            LampDetails_LampBaseType = (uint)LSFLampBaseType.BASETYPE_INVALID;
            LampDetails_LampID = device.Model + device.SerialNumber;
            LampDetails_LampType = (uint)LSFLampType.LAMPTYPE_INVALID;
            LampDetails_Make = (uint)AdapterLib.LsfEnums.LampMake.MAKE_OEM1;
            LampDetails_Model = 1;
            LampDetails_Type = (uint)AdapterLib.LsfEnums.DeviceType.TYPE_OUTLET;
            
        }

        public bool LampDetails_Color { get; }
        public uint LampDetails_ColorRenderingIndex { get; }
        public bool LampDetails_Dimmable { get; }
        public bool LampDetails_HasEffects { get; }
        public uint LampDetails_IncandescentEquivalent { get; }
        public uint LampDetails_LampBaseType { get; }
        public uint LampDetails_LampBeamAngle { get; }
        public string LampDetails_LampID { get; }
        public uint LampDetails_LampType { get; }
        public uint LampDetails_Make { get; }
        public uint LampDetails_MaxLumens { get; }
        public uint LampDetails_MaxTemperature { get; }
        public uint LampDetails_MaxVoltage { get; }
        public uint LampDetails_MinTemperature { get; }
        public uint LampDetails_MinVoltage { get; }
        public uint LampDetails_Model { get; }
        public uint LampDetails_Type { get; }
        public bool LampDetails_VariableColorTemp { get; }
        public uint LampDetails_Version { get; }
        public uint LampDetails_Wattage { get; }
        public uint LampParameters_BrightnessLumens { get; }
        public uint LampParameters_EnergyUsageMilliwatts { get; }
        public uint LampParameters_Version { get; }
        public uint[] LampService_LampFaults { get; }
        public uint LampService_LampServiceVersion { get; }
        public uint LampService_Version { get; }
        private uint lampState_Brightness = uint.MinValue;
        public uint LampState_Brightness
        {
            get { return lampState_Brightness; }
            set
            {
                if (device.Type == TypeEnum.SelflearningDimmer)
                {
                    byte dimlevel = (byte) (255 * ((decimal)value / (decimal)uint.MaxValue));
                    string command = protocol.GetString(device.SerialNumber, device.Type, MethodEnum.DIM,(char) dimlevel);
                    tellstickDevice.SendCommand(command);
                    lampState_Brightness = value;
                }
            }
        }
        public uint LampState_ColorTemp { get; set; }
        public uint LampState_Hue { get; set; }
        private AdapterSignal _LampStateChanged = new AdapterSignal(Constants.LAMP_STATE_CHANGED_SIGNAL_NAME);
 
        public IAdapterSignal LampState_LampStateChanged 
        { 
                get 
                { 
                    return _LampStateChanged; 
                } 
        }
        
        private bool lampState_OnOff =false;
        public bool LampState_OnOff
        {
            get { return lampState_OnOff; }
            set
            {
                string command=protocol.GetString(device.SerialNumber,device.Type, value?MethodEnum.TURNON: MethodEnum.TURNOFF,device.Data);
                tellstickDevice.SendCommand(command);
                lampState_OnOff = value;
                   
            }
        }
        public uint LampState_Saturation { get; set; }
        public uint LampState_Version { get; set; }
        public uint ClearLampFault(uint InLampFaultCode, out uint LampResponseCode, out uint OutLampFaultCode)
        {
            LampResponseCode=OutLampFaultCode = 0;
            return 0;
        }

        public uint LampState_ApplyPulseEffect(State FromState, State ToState, uint Period, uint Duration, uint NumPulses, ulong Timestamp, out uint LampResponseCode)
        {
            LampResponseCode = 0;
            return 0;
        }

        public uint TransitionLampState(ulong Timestamp, State NewState, uint TransitionPeriod, out uint LampResponseCode)
        {
            LampResponseCode = 0;
            return 0;
        }
    }
}
