using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLib
{
    class NamespaceAttribute:Attribute
    {
        public string Namespace { get; set; }

        public NamespaceAttribute(string ns)
        {
            Namespace = ns;
        }
    }
}
