using BridgeRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLib
{
    class SignalBehaviorAttribute : Attribute
    {
        public SignalBehavior SignalBehavior { get; set; }

        public SignalBehaviorAttribute(SignalBehavior signalBehavior)
        {
            SignalBehavior = signalBehavior;
        }
    }
}
