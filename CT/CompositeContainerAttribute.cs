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
        public CompositeContainerAttribute(string compositeContainerDictionaryPropertyName, string modelDictionaryPropertyName)
        {
            CompositeContainerDictionaryPropertyName = string.IsNullOrEmpty(compositeContainerDictionaryPropertyName) ? throw new ArgumentException(Resources.MustSupplyCompositeContainerDictionaryPropertyName) : compositeContainerDictionaryPropertyName;
            ModelDictionaryPropertyName = string.IsNullOrEmpty(modelDictionaryPropertyName) ? throw new ArgumentException(Resources.MustSupplyModelDictionaryPropertyName) : modelDictionaryPropertyName;
        }

        public string CompositeContainerDictionaryPropertyName { get; private set; }

        public string ModelDictionaryPropertyName { get; private set; }
    }
}
