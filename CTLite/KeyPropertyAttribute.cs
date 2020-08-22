using CTLite.Properties;
using System;

namespace CTLite
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class KeyPropertyAttribute : Attribute
    {
        public KeyPropertyAttribute(string propertyName)
        {
            PropertyName = string.IsNullOrEmpty(propertyName) ? throw new ArgumentException(Resources.MustSupplyPropertyName) : propertyName;
        }

        public string PropertyName { get; private set; }
    }
}
