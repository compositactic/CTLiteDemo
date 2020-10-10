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
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract CompositeState State { get; set; }

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            var property = GetType().GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException(propertyName);

            State = State == CompositeState.New ? CompositeState.New : CompositeState.Modified;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [DataMember]
        [NoDb]
        public string Path { get { return this.GetPath(); } }

        public CompositeRoot CompositeRoot
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
                .ForEach
                (
                    pi => compositePropertyInfos
                        .Add
                        (
                            new CompositePropertyInfo
                            (
                                pi.Name,
                                pi.PropertyType,
                                pi.GetSetMethod(false) == null && (pi.GetCustomAttribute<PresentationStateControlAttribute>() == null || GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().IsReadOnlyMethodName) == null || (bool)GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().IsReadOnlyMethodName).Invoke(this, null)),
                                pi.GetCustomAttribute<HelpAttribute>()?.Text,
                                pi.GetCustomAttribute<PresentationStateControlAttribute>() == null || GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().IsVisibleMethodName) == null || (bool)GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().IsVisibleMethodName).Invoke(this, null),
                                pi.GetCustomAttribute<PresentationStateControlAttribute>() == null || GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().IsEnabledMethodName) == null || (bool)GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().IsEnabledMethodName).Invoke(this, null),
                                pi.GetCustomAttribute<PresentationStateControlAttribute>() == null || GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().PresentationDataMethodName) == null || (bool)GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().PresentationDataMethodName).Invoke(this, null),
                                pi.GetCustomAttribute<PresentationStateControlAttribute>() == null || GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().PresentationLabelDataMethodName) == null || (bool)GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().PresentationLabelDataMethodName).Invoke(this, null)
                            )
                        )
                );


            var compositeCommandInfos = new List<CompositeCommandInfo>();
            var compositeCommandParameterInfos = new List<CompositeCommandParameterInfo>();
            foreach (var cmi in compositeMemberInfos.Where(mi => mi.MemberType == MemberTypes.Method).Cast<MethodInfo>())
            {
                PresentationStateControlAttribute psc;
                var isMethodEnabled = true;
                var isMethodVisible = true;
                var presentationData = new object();
                var presentationLabelData = new object();

                if ((psc = cmi.GetCustomAttribute<PresentationStateControlAttribute>()) != null)
                {
                    isMethodVisible = string.IsNullOrEmpty(psc.IsVisibleMethodName) || GetType().GetMethod(psc.IsVisibleMethodName) == null || (bool)GetType().GetMethod(psc.IsVisibleMethodName).Invoke(this, null);
                    isMethodEnabled = string.IsNullOrEmpty(psc.IsEnabledMethodName) || GetType().GetMethod(psc.IsEnabledMethodName) == null || (bool)GetType().GetMethod(psc.IsEnabledMethodName).Invoke(this, null);
                    presentationData = !string.IsNullOrEmpty(psc.PresentationDataMethodName) && GetType().GetMethod(psc.PresentationDataMethodName) != null ?
                                            GetType().GetMethod(psc.PresentationDataMethodName).Invoke(this, null) : null;
                    presentationLabelData = !string.IsNullOrEmpty(psc.PresentationLabelDataMethodName) && GetType().GetMethod(psc.PresentationLabelDataMethodName) != null ?
                        GetType().GetMethod(psc.PresentationLabelDataMethodName).Invoke(this, null) : null;
                }

                foreach (var parameterInfo in cmi.GetParameters().Where(pi => pi.ParameterType != typeof(CompositeRootHttpContext)))
                    compositeCommandParameterInfos.Add(new CompositeCommandParameterInfo(parameterInfo.Name, parameterInfo.ParameterType, parameterInfo.GetCustomAttribute<HelpAttribute>()?.Text, parameterInfo.ParameterType.GetTypeEnumValues()));

                var returnValueHelpText = cmi.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(HelpAttribute), true).Cast<HelpAttribute>().FirstOrDefault()?.Text;

                compositeCommandInfos.Add(new CompositeCommandInfo
                (
                    cmi.Name,
                    cmi.GetCustomAttribute<HelpAttribute>()?.Text,
                    compositeCommandParameterInfos,
                    cmi.ReturnType, 
                    returnValueHelpText, 
                    isMethodVisible,
                    isMethodEnabled,
                    presentationData,
                    presentationLabelData));

                compositeCommandParameterInfos.Clear();
            }

            return new CompositeMemberInfo(compositePropertyInfos, compositeCommandInfos, this.GetPath());
        }
    }
}
