using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLib
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    class AnnotationAttribute: Attribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public AnnotationAttribute(string name,string value)
        {
            Name = name;
            Value = value;
        }
    }
}
