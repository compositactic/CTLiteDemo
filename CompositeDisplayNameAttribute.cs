using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace CTLite
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CompositeDisplayNameAttribute : DisplayAttribute
    {
        public CompositeCategoryAttribute(string category) : base(category)
        {
            Category = category;
        }

        public CompositeCategoryAttribute(Type resourceType, string resourceName)
        {
            ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            Category = resourceType.GetProperty(resourceName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string;
        }

        public CompositeCategoryAttribute(Type resourceType, string resourceName, params object[] resourceNameStringArgs)
        {
            ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            Category = string.Format(CultureInfo.CurrentCulture, resourceType.GetProperty(resourceName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string, resourceNameStringArgs);
        }

        public new string Category { get; }

        public Type ResourceType { get; }

        public string ResourceName { get; }

        protected override string GetLocalizedString(string value)
        {
            var localizedString = ResourceType.GetProperty(value, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string;
            return localizedString == null ? base.GetLocalizedString(value) : localizedString;
        }
    }
}


