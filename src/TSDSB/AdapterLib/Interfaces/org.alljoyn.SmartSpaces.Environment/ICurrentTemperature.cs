using AdapterLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.alljoyn.SmartSpaces.Environment
{
    [Namespace("org.alljoyn.SmartSpaces.Environment.CurrentTemperature")]
    [Annotation("org.alljoyn.Bus.DocString.En", "This interface provides capability to represent current temperature.")]
    [Annotation("org.alljoyn.Bus.Secure", "true")]
    internal interface ICurrentTemperature
    {
        [Annotation("org.alljoyn.Bus.DocString.En", "The interface version")]
        [Annotation("org.freedesktop.DBus.Property.EmitsChangedSignal", "const")]
        short Version { get; }

        [Annotation("org.alljoyn.Bus.DocString.En", "Current temperature expressed in Celsius.")]
        [Annotation("org.freedesktop.DBus.Property.EmitsChangedSignal", "true")]
        [Annotation("org.alljoyn.Bus.Type.Units", "degrees Celsius")]
        double CurrentValue { get; }

        [Annotation("org.alljoyn.Bus.DocString.En", "The precision of the CurrentValue property. i.e. the number of degrees Celsius the actual power consumption must change before CurrentValue is updated.")]
        [Annotation("org.freedesktop.DBus.Property.EmitsChangedSignal", "true")]
        [Annotation("org.alljoyn.Bus.Type.Units", "degrees Celsius")]
        double Precision { get; }


        [Annotation("org.alljoyn.Bus.DocString.En", "The minimum time between updates of the CurrentValue property in milliseconds.")]
        [Annotation("org.freedesktop.DBus.Property.EmitsChangedSignal", "true")]
        [Annotation("org.alljoyn.Bus.Type.Units", "milliseconds")]
        ushort UpdateMinTime { get; }
    }
}
