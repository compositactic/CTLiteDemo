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
            PresentationStateControlAttribute presentationStateControlAttribute;
            CategoryAttribute categoryAttribute;
            DisplayNameAttribute displayNameAttribute;

            var isMethodEnabled = true;
            var isMethodVisible = true;
            var presentationData = new object();
            var presentationLabelData = new object();
            var isReadOnly = false;
            var labelText = (string)null;
            var category = (string)null;

            var compositeMemberInfos =
                GetType()
                .GetMembers(_flags)
                .Where(mi => mi.GetCustomAttribute<DataMemberAttribute>() != null || mi.GetCustomAttribute<CommandAttribute>() != null);

            var compositePropertyInfos = new List<CompositePropertyInfo>();

            foreach(var pi in compositeMemberInfos.Where(mi => mi.MemberType == MemberTypes.Property).Cast<PropertyInfo>())
            {
                if ((presentationStateControlAttribute = pi.GetCustomAttribute<PresentationStateControlAttribute>()) != null)
                {
                    GetPresentationControlData(presentationStateControlAttribute, out isMethodEnabled, out isMethodVisible, out presentationData, out presentationLabelData);
                    isReadOnly = pi.GetSetMethod(false) == null && (string.IsNullOrEmpty(pi.GetCustomAttribute<PresentationStateControlAttribute>().IsReadOnlyMethodName) || GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().IsReadOnlyMethodName) == null || (bool)GetType().GetMethod(pi.GetCustomAttribute<PresentationStateControlAttribute>().IsReadOnlyMethodName).Invoke(this, null));
                }

                if((categoryAttribute = pi.GetCustomAttribute<CategoryAttribute>(true)) != null)
                    category = categoryAttribute.Category;

                if ((displayNameAttribute = pi.GetCustomAttribute<DisplayNameAttribute>(true)) != null)
                    labelText = displayNameAttribute.DisplayName;

                compositePropertyInfos.Add
                (
                    new CompositePropertyInfo
                    (
                        pi.Name,
                        pi.PropertyType,
                        isReadOnly,
                        pi.GetCustomAttribute<HelpAttribute>()?.Text,
                        isMethodVisible,
                        isMethodEnabled,
                        presentationData,
                        presentationLabelData,
                        labelText,
                        category
                    )
                );
            }

            var compositeCommandInfos = new List<CompositeCommandInfo>();
            var compositeCommandParameterInfos = new List<CompositeCommandParameterInfo>();
            foreach (var cmi in compositeMemberInfos.Where(mi => mi.MemberType == MemberTypes.Method).Cast<MethodInfo>())
            {
                if ((presentationStateControlAttribute = cmi.GetCustomAttribute<PresentationStateControlAttribute>()) != null)
                    GetPresentationControlData(presentationStateControlAttribute, out isMethodEnabled, out isMethodVisible, out presentationData, out presentationLabelData);

                foreach (var parameterInfo in cmi.GetParameters().Where(pi => pi.ParameterType != typeof(CompositeRootHttpContext)))
                    compositeCommandParameterInfos.Add(new CompositeCommandParameterInfo(parameterInfo.Name, parameterInfo.ParameterType, parameterInfo.GetCustomAttribute<HelpAttribute>()?.Text, parameterInfo.ParameterType.GetTypeEnumValues()));

                var returnValueHelpText = cmi.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(HelpAttribute), true).Cast<HelpAttribute>().FirstOrDefault()?.Text;

                if ((categoryAttribute = cmi.GetCustomAttribute<CategoryAttribute>(true)) != null)
                    category = categoryAttribute.Category;

                if ((displayNameAttribute = cmi.GetCustomAttribute<DisplayNameAttribute>(true)) != null)
                    labelText = displayNameAttribute.DisplayName;

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
                    presentationLabelData,
                    labelText,
                    category));

                compositeCommandParameterInfos.Clear();
            }

            return new CompositeMemberInfo(compositePropertyInfos, compositeCommandInfos, this.GetPath());
        }

        private void GetPresentationControlData(PresentationStateControlAttribute psc, out bool isMethodEnabled, out bool isMethodVisible, out object presentationData, out object presentationLabelData)
        {
            isMethodVisible = string.IsNullOrEmpty(psc.IsVisibleMethodName) || GetType().GetMethod(psc.IsVisibleMethodName) == null || (bool)GetType().GetMethod(psc.IsVisibleMethodName).Invoke(this, null);
            isMethodEnabled = string.IsNullOrEmpty(psc.IsEnabledMethodName) || GetType().GetMethod(psc.IsEnabledMethodName) == null || (bool)GetType().GetMethod(psc.IsEnabledMethodName).Invoke(this, null);
            presentationData = !string.IsNullOrEmpty(psc.PresentationDataMethodName) && GetType().GetMethod(psc.PresentationDataMethodName) != null ?
                                    GetType().GetMethod(psc.PresentationDataMethodName).Invoke(this, null) : null;
            presentationLabelData = !string.IsNullOrEmpty(psc.PresentationLabelDataMethodName) && GetType().GetMethod(psc.PresentationLabelDataMethodName) != null ?
                GetType().GetMethod(psc.PresentationLabelDataMethodName).Invoke(this, null) : null;
        }
    }
}
