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

using CTLite.Properties;
using System;
using System.Globalization;

namespace CTLite
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CompositeContainerAttribute : Attribute
    {
        public CompositeContainerAttribute(string compositeContainerDictionaryPropertyName)
        {
            CompositeContainerDictionaryPropertyName = string.IsNullOrEmpty(compositeContainerDictionaryPropertyName) ? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidPropertyName, string.Empty)) : compositeContainerDictionaryPropertyName;
        }
        public CompositeContainerAttribute(string compositeContainerDictionaryPropertyName, string modelDictionaryPropertyName, string internalCompositeContainerDictionaryPropertyName)
        {
            CompositeContainerDictionaryPropertyName = string.IsNullOrEmpty(compositeContainerDictionaryPropertyName) ? throw new ArgumentException(Resources.MustSupplyCompositeContainerDictionaryPropertyName) : compositeContainerDictionaryPropertyName;
            ModelDictionaryPropertyName = string.IsNullOrEmpty(modelDictionaryPropertyName) ? throw new ArgumentException(Resources.MustSupplyModelDictionaryPropertyName) : modelDictionaryPropertyName;
            InternalCompositeContainerDictionaryPropertyName = string.IsNullOrEmpty(internalCompositeContainerDictionaryPropertyName) ? throw new ArgumentException(Resources.MustSupplyInternalCompositeContainerDictionaryPropertyName) : internalCompositeContainerDictionaryPropertyName;
        }

        public string CompositeContainerDictionaryPropertyName { get; private set; }

        public string ModelDictionaryPropertyName { get; private set; }

        public string InternalCompositeContainerDictionaryPropertyName { get; private set; }
    }
}
