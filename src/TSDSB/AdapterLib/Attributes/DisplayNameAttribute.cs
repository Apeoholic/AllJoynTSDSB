using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLib
{
    class DisplayNameAttribute:Attribute
    {
        public string Name { get; set; }
        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}
