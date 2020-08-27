using CTLite.Properties;
using System;

namespace CTLite
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class KeyPropertyAttribute : Attribute
    {
        public KeyPropertyAttribute(string keyPropertyName)
        {
            KeyPropertyName = string.IsNullOrEmpty(keyPropertyName) ? throw new ArgumentException(Resources.MustSupplyPropertyName) : keyPropertyName;
        }

        public KeyPropertyAttribute(string keyPropertyName, string originalKeyPropertyName)
        {
            KeyPropertyName = string.IsNullOrEmpty(keyPropertyName) ? throw new ArgumentException(Resources.MustSupplyPropertyName) : keyPropertyName;
            OriginalKeyPropertyName = string.IsNullOrEmpty(originalKeyPropertyName) ? throw new ArgumentException() : originalKeyPropertyName;
        }

        public string KeyPropertyName { get; private set; }

        public string OriginalKeyPropertyName { get; private set; }

    }
}
