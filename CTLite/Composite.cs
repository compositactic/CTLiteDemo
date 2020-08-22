using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace CTLite
{
    [DataContract]
    public abstract class Composite : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler _propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }

        public CompositeState State { get; set; } = CompositeState.Unchanged;

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            var property = GetType().GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException(propertyName);
        }

        protected CompositeRoot CompositeRoot
        {
            get { return GetParentComposite(this, null, null) as CompositeRoot; }
        }

        private Composite GetParentComposite(Composite composite, Type parentCompositeType, string parentPropertyName)
        {
            if (composite == null)
                return null;

            if (composite is CompositeRoot)
                return composite as CompositeRoot;
            else
            {
                var parentPropertyAttribute = composite.GetType().FindCustomAttribute<ParentPropertyAttribute>();
                if (parentPropertyAttribute == null)
                    return null;

                var parentPropertyInfo = composite.GetType().GetProperty(parentPropertyAttribute.ParentPropertyName);
                var parentComposite = parentPropertyInfo.GetValue(composite) as Composite;
                if (parentPropertyAttribute.ParentPropertyName == parentPropertyName && parentPropertyInfo.PropertyType == parentCompositeType)
                    return parentComposite;

                return GetParentComposite(parentComposite, parentCompositeType, parentPropertyName);
            }
        }

        private readonly BindingFlags _flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.InvokeMethod;

        internal CompositeMemberInfo GetCompositeMemberInfo()
        {
            var compositeMemberInfos =
                GetType()
                .GetMembers(_flags)
                .Where(mi => mi.GetCustomAttribute<DataMemberAttribute>() != null || mi.GetCustomAttribute<CommandAttribute>() != null);

            var compositePropertyInfos = new List<CompositePropertyInfo>();

            compositeMemberInfos
                .Where(mi => mi.MemberType == MemberTypes.Property)
                .Cast<PropertyInfo>().ToList()
                .ForEach(pi => compositePropertyInfos.Add(new CompositePropertyInfo(pi.Name, pi.PropertyType, pi.GetSetMethod(false) == null, pi.GetCustomAttribute<HelpAttribute>()?.Text)));

            var compositeCommandInfos = new List<CompositeCommandInfo>();
            var compositeCommandParameterInfos = new List<CompositeCommandParameterInfo>();
            foreach (var cmi in compositeMemberInfos.Where(mi => mi.MemberType == MemberTypes.Method).Cast<MethodInfo>())
            {
                foreach (var parameterInfo in cmi.GetParameters().Where(pi => pi.ParameterType != typeof(CompositeRootHttpContext)))
                    compositeCommandParameterInfos.Add(new CompositeCommandParameterInfo(parameterInfo.Name, parameterInfo.ParameterType, parameterInfo.GetCustomAttribute<HelpAttribute>()?.Text, parameterInfo.ParameterType.GetTypeEnumValues()));

                var returnValueHelpText = cmi.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(HelpAttribute), true).Cast<HelpAttribute>().FirstOrDefault()?.Text;

                compositeCommandInfos.Add(new CompositeCommandInfo(cmi.Name, cmi.GetCustomAttribute<HelpAttribute>()?.Text, compositeCommandParameterInfos, cmi.ReturnType, returnValueHelpText));
            }

            return new CompositeMemberInfo(compositePropertyInfos, compositeCommandInfos);
        }
    }
}
