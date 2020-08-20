using CTLite.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace CTLite
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ParentPropertyAttribute : Attribute
    {
        public ParentPropertyAttribute(string parentPropertyName)
        {
            ParentPropertyName = string.IsNullOrEmpty(parentPropertyName) ? throw new ArgumentException(Resources.MustSupplyParentPropertyName) : parentPropertyName;
        }

        public ParentPropertyAttribute(string parentPropertyName, string parentCompositePropertyName)
        {
            ParentPropertyName = string.IsNullOrEmpty(parentPropertyName) ? throw new ArgumentException(Resources.MustSupplyParentPropertyName) : parentPropertyName;
            ParentCompositePropertyName = string.IsNullOrEmpty(parentCompositePropertyName) ? throw new ArgumentException(Resources.MustSupplyParentCompositePropertyName) : parentCompositePropertyName;
        }

        public string ParentPropertyName { get; private set; }
        public string ParentCompositePropertyName { get; private set; }
    }
}
