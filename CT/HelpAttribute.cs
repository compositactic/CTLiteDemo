using System;
using System.Globalization;
using System.Reflection;

namespace CTLite
{
    [AttributeUsage(AttributeTargets.Method |
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Parameter |
        AttributeTargets.ReturnValue,
        Inherited = false, AllowMultiple = false)]
    public sealed class HelpAttribute : Attribute
    {
        public HelpAttribute(string text)
        {
            Text = text;
        }

        public HelpAttribute(Type resourceType, string resourceName)
        {
            ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            Text = resourceType.GetProperty(resourceName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string;
        }

        public HelpAttribute(Type resourceType, string resourceName, params object[] resourceNameStringArgs)
        {
            ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            Text = string.Format(CultureInfo.CurrentCulture, resourceType.GetProperty(resourceName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string, resourceNameStringArgs);
        }

        public string Text { get; }

        public Type ResourceType { get; }

        public string ResourceName { get; }
    }
}