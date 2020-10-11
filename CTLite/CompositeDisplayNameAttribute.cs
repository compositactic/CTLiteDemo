using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace CTLite
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CompositeDisplayNameAttribute : DisplayNameAttribute
    {
        public CompositeDisplayNameAttribute(string displayName) : base(displayName)
        {
            DisplayName = displayName;
        }

        public CompositeDisplayNameAttribute(Type resourceType, string resourceName)
        {
            ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            DisplayName = resourceType.GetProperty(resourceName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string;
        }

        public CompositeDisplayNameAttribute(Type resourceType, string resourceName, params object[] resourceNameStringArgs)
        {
            ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            DisplayName = string.Format(CultureInfo.CurrentCulture, resourceType.GetProperty(resourceName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string, resourceNameStringArgs);
        }

        public new string DisplayName { get; }

        public Type ResourceType { get; }

        public string ResourceName { get; }

    }
}
