using CTLite.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace CTLite
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CompositeModelAttribute : Attribute
    {
        public CompositeModelAttribute(string modelFieldName)
        {
            ModelFieldName = string.IsNullOrEmpty(modelFieldName) ? throw new ArgumentException(Resources.MustSupplyModelFieldName) : modelFieldName;
        }

        public string ModelFieldName { get; set; }
    }
}
