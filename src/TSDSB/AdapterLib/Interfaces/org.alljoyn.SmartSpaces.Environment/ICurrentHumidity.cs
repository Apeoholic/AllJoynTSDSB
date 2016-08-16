using AdapterLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.alljoyn.SmartSpaces.Environment
{
    [Namespace("org.alljoyn.SmartSpaces.Environment.CurrentHumidity")]
    [Annotation("org.alljoyn.Bus.DocString.En", "This interface provides capability to represent current relative humidity.")]
    [Annotation("org.alljoyn.Bus.Secure", "true")]
    internal interface ICurrentHumidity
    {
        [Annotation("org.alljoyn.Bus.DocString.En", "The interface version")]
        [Annotation("org.freedesktop.DBus.Property.EmitsChangedSignal", "const")]
        short Version { get; }

        [Annotation("org.alljoyn.Bus.DocString.En", "Current relative humidity value.")]
        [Annotation("org.freedesktop.DBus.Property.EmitsChangedSignal", "true")]
        [Annotation("org.alljoyn.Bus.Type.Min", "0")]
        double CurrentValue { get; }

        
        [Annotation("org.alljoyn.Bus.DocString.En", "Maximum value allowed for represented relative humidity.")]
        [Annotation("org.freedesktop.DBus.Property.EmitsChangedSignal", "true")]
        double MaxValue { get; }
    }
}
