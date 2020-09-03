// CTLite - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
// NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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