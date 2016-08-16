using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BridgeRT;
using System.Reflection;
using System.Diagnostics;
using Windows.Devices.AllJoyn;
using System.ComponentModel;

namespace AdapterLib
{
    //
    // AdapterValue.
    // Description:
    // The class that implements IAdapterValue from BridgeRT.
    //
    class AdapterValue : IAdapterValue
    {
        // public properties
        public string Name { get; }
        public object Data { get; set; }

        internal AdapterValue(string ObjectName, object DefaultData)
        {
            this.Name = ObjectName;
            this.Data = DefaultData;
        }

        internal AdapterValue(AdapterValue Other)
        {
            this.Name = Other.Name;
            this.Data = Other.Data;
        }
    }

    //
    // AdapterProperty.
    // Description:
    // The class that implements IAdapterProperty from BridgeRT.
    //
    class AdapterProperty : IAdapterProperty
    {
        // public properties
        public string Name { get; }
        public string InterfaceHint { get; }
        public IList<IAdapterAttribute> Attributes { get; }

        internal PropertyInfo PropertyInfo { get; set; }
        internal object Device { get; set; }


        internal AdapterProperty(string ObjectName, string IfHint)
        {
            this.Name = ObjectName;
            this.InterfaceHint = IfHint;

            try
            {
                this.Attributes = new List<IAdapterAttribute>();
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal AdapterProperty(AdapterProperty Other)
        {
            this.Name = Other.Name;
            this.InterfaceHint = Other.InterfaceHint;

            try
            {
                this.Attributes = new List<IAdapterAttribute>(Other.Attributes);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }
    }

    //
    // AdapterAttribute.
    // Description:
    // The class that implements IAdapterAttribute from BridgeRT.
    //
    class AdapterAttribute : IAdapterAttribute
    {
        // public properties
        public IAdapterValue Value { get; }

        public E_ACCESS_TYPE Access { get; set; }
        public IDictionary<string, string> Annotations { get; }
        public SignalBehavior COVBehavior { get; set; }

        internal AdapterAttribute(string ObjectName, object DefaultData, E_ACCESS_TYPE access = E_ACCESS_TYPE.ACCESS_READ)
        {
            try
            {
                this.Value = new AdapterValue(ObjectName, DefaultData);
                this.Annotations = new Dictionary<string, string>();
                this.Access = access;
                this.COVBehavior = SignalBehavior.Never;
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal AdapterAttribute(AdapterAttribute Other)
        {
            this.Value = Other.Value;
            this.Annotations = Other.Annotations;
            this.Access = Other.Access;
            this.COVBehavior = Other.COVBehavior;
        }
    }

    //
    // AdapterMethod.
    // Description:
    // The class that implements IAdapterMethod from BridgeRT.
    //
    class AdapterMethod : IAdapterMethod
    {
        // public properties
        public string Name { get; }

        public string Description { get; }

        public IList<IAdapterValue> InputParams { get; set; }

        public IList<IAdapterValue> OutputParams { get; }

        public int HResult { get; private set; }

        internal AdapterMethod(
            string ObjectName,
            string Description,
            int ReturnValue)
        {
            this.Name = ObjectName;
            this.Description = Description;
            this.HResult = ReturnValue;

            try
            {
                this.InputParams = new List<IAdapterValue>();
                this.OutputParams = new List<IAdapterValue>();
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal AdapterMethod(AdapterMethod Other)
        {
            this.Name = Other.Name;
            this.Description = Other.Description;
            this.HResult = Other.HResult;

            try
            {
                this.InputParams = new List<IAdapterValue>(Other.InputParams);
                this.OutputParams = new List<IAdapterValue>(Other.OutputParams);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal void SetResult(int ReturnValue)
        {
            this.HResult = ReturnValue;
        }

        internal MethodInfo MethodInfo { get; set; }
        internal object Device { get; set; }
    }

    //
    // AdapterSignal.
    // Description:
    // The class that implements IAdapterSignal from BridgeRT.
    //
    class AdapterSignal : IAdapterSignal
    {
        // public properties
        public string Name { get; }

        public IList<IAdapterValue> Params { get; }

        internal AdapterSignal(string ObjectName)
        {
            this.Name = ObjectName;

            try
            {
                this.Params = new List<IAdapterValue>();
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal AdapterSignal(AdapterSignal Other)
        {
            this.Name = Other.Name;

            try
            {
                this.Params = new List<IAdapterValue>(Other.Params);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }
    }

    //
    // AdapterDevice.
    // Description:
    // The class that implements IAdapterDevice from BridgeRT.
    //
    class AdapterDevice : IAdapterDevice,
                            IAdapterDeviceLightingService,
                            IAdapterDeviceControlPanel
    {


        public Object Device { get; set; }
        public Adapter Adapter { get; set; }
        public void Initialize()
        {

            foreach (var m in Device.GetType().GetMethods())
            {
                if (m.DeclaringType.FullName != "System.Object" && m.IsPublic && (m.Attributes & MethodAttributes.SpecialName) != MethodAttributes.SpecialName)
                {
                    var name = m.Name;
                    var displayName = m.GetCustomAttribute<DisplayNameAttribute>();
                    if (displayName != null)
                    {
                        name = displayName.Name;
                    }

                    Debug.WriteLine(m.Name);
                    var method = new AdapterMethod(name, "", 0);
                    method.Device = this.Device;
                    method.MethodInfo = m;

                    this.Methods.Add(method);
                }
            }


            //Properties
            var interfaces = Device.GetType().GetInterfaces();
            foreach (var i in interfaces)
            {
                if (i.Name == "INotifyPropertyChanged")
                {
                    var pc = Device as INotifyPropertyChanged;
                    pc.PropertyChanged += Pc_PropertyChanged;
                    continue;
                }

                var name = i.Name;
                //get the interface
                var currentInterface = Device.GetType().GetTypeInfo().ImplementedInterfaces.FirstOrDefault(ii => ii.Name == name);
                if (currentInterface != null)
                {
                    var ns = currentInterface.GetTypeInfo().GetCustomAttribute<NamespaceAttribute>();
                    if (ns != null)
                    {
                        var property = new AdapterProperty(ns.Namespace, ns.Namespace);
                        foreach (var p in i.GetProperties())
                        {
                            property.PropertyInfo = p;
                            property.Device = this.Device;

                            if (p.DeclaringType.FullName != "System.Object")
                            {
                                E_ACCESS_TYPE accessType = E_ACCESS_TYPE.ACCESS_READ;
                                if (p.CanWrite)
                                {
                                    accessType = E_ACCESS_TYPE.ACCESS_WRITE;
                                }
                                if (p.CanWrite && p.CanWrite)
                                {
                                    accessType = E_ACCESS_TYPE.ACCESS_READWRITE;
                                }

                                var aa = new AdapterAttribute(p.Name, p.GetValue(Device), accessType);
                                var annotations = p.GetCustomAttributes<AnnotationAttribute>();
                                foreach (var annotation in annotations)
                                {
                                    if (annotation.Name == "org.freedesktop.DBus.Property.EmitsChangedSignal")
                                    {
                                        if (annotation.Value == "true")
                                        {
                                            aa.COVBehavior = SignalBehavior.Always;
                                        }
                                        continue;
                                    }
                                    aa.Annotations.Add(annotation.Name, annotation.Value);
                                }
                                property.Attributes.Add(aa);
                            }
                        }
                        Properties.Add(property);
                    }
                }
            }

            //TODO:Implement Events
            //foreach (var ev in Device.GetType().GetEvents())
            //{
            //    var displayName = ev.GetCustomAttribute<DisplayNameAttribute>();
            //    if (displayName != null)
            //    {
            //        Debug.WriteLine(displayName.DisplayName);
            //    }
            //}

            CreateEmitSignalChangedSignal();

        }

        private void Pc_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var p in Properties)
            {
                //Find what interface and property has been updated
                foreach (AdapterAttribute a in p.Attributes)
                {

                    var propInterfaceName = ((AdapterLib.AdapterProperty)p).PropertyInfo.DeclaringType.Name;
                     //Find the actual property on the class
                     var interfaces = Device.GetType().GetInterfaces().Where(di=>di.Name== propInterfaceName);
                    foreach (var i in interfaces)
                    {
                        if (i.Name == "INotifyPropertyChanged")
                            continue;

                        if (i.Name + "." + a.Value.Name == e.PropertyName)
                        {
                            var currentInterface = Device.GetType().GetTypeInfo().ImplementedInterfaces.FirstOrDefault(ii => ii.Name == i.Name);
                            if (currentInterface != null)
                            {
                                a.Value.Data = currentInterface.GetProperty(a.Value.Name).GetGetMethod().Invoke(Device, null);
                                Adapter.SignalChangeOfAttributeValue(this, p, a);
                            }
                        }
                    }

                }
            }
        }

        private void CreateEmitSignalChangedSignal()
        {
            // change of value signal

            AdapterSignal changeOfAttributeValue = new AdapterSignal(Constants.CHANGE_OF_VALUE_SIGNAL);
            changeOfAttributeValue.Params.Add(new AdapterValue(Constants.COV__PROPERTY_HANDLE, null));
            changeOfAttributeValue.Params.Add(new AdapterValue(Constants.COV__ATTRIBUTE_HANDLE, null));
            Signals.Add(changeOfAttributeValue);
        }

        // Object Name
        public string Name { get; }

        // Device information
        public string Vendor { get; }

        public string Model { get; }

        public string Version { get; } = "";

        public string SupportedMethods { get; }

        public char Data { get; set; }

        public string FirmwareVersion { get; }

        public string SerialNumber { get; }

        public string Description { get; }

        // Device properties
        public IList<IAdapterProperty> Properties { get; }

        // Device methods
        public IList<IAdapterMethod> Methods { get; }

        // Device signals
        public IList<IAdapterSignal> Signals { get; }

        // Control Panel Handler
        public IControlPanelHandler ControlPanelHandler
        {
            get
            {
                return null;
            }
        }

        // Lighting Service Handler
        public ILSFHandler LightingServiceHandler
        {
            get;set;
        }

        // Icon
        public IAdapterIcon Icon
        {
            get;  set;
        }


        internal AdapterDevice(
            string Name,
            string VendorName,
            string Model,
            string SupportedMethods,
            string Version,
            string SerialNumber,
            string Description, Adapter adapter)
        {
            this.Name = Name;
            this.Vendor = VendorName;
            this.Model = Model;
            this.SupportedMethods = SupportedMethods;
            this.Version = Version;
            this.FirmwareVersion = Version;
            this.SerialNumber = SerialNumber;
            this.Description = Description;
            this.Adapter = adapter;

            try
            {
                this.Properties = new List<IAdapterProperty>();
                this.Methods = new List<IAdapterMethod>();
                this.Signals = new List<IAdapterSignal>();
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal AdapterDevice(AdapterDevice Other)
        {
            this.Name = Other.Name;
            this.Vendor = Other.Vendor;
            this.Model = Other.Model;
            this.Version = Other.Version;
            this.FirmwareVersion = Other.FirmwareVersion;
            this.SerialNumber = Other.SerialNumber;
            this.Description = Other.Description;

            try
            {
                this.Properties = new List<IAdapterProperty>(Other.Properties);
                this.Methods = new List<IAdapterMethod>(Other.Methods);
                this.Signals = new List<IAdapterSignal>(Other.Signals);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal void AddChangeOfValueSignal(
            IAdapterProperty Property,
            IAdapterValue Attribute)
        {
            try
            {
                AdapterSignal covSignal = new AdapterSignal(Constants.CHANGE_OF_VALUE_SIGNAL);

                // Property Handle
                AdapterValue propertyHandle = new AdapterValue(
                                                    Constants.COV__PROPERTY_HANDLE,
                                                    Property);

                // Attribute Handle
                AdapterValue attrHandle = new AdapterValue(
                                                    Constants.COV__ATTRIBUTE_HANDLE,
                                                    Attribute);

                covSignal.Params.Add(propertyHandle);
                covSignal.Params.Add(attrHandle);

                this.Signals.Add(covSignal);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }
    }
}
