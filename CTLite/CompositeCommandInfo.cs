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
using System.Linq;
using System.Runtime.Serialization;

namespace CTLite
{
    [DataContract]
    [Serializable]
    public class CompositeCommandInfo
    {
        internal CompositeCommandInfo(string commandName, string helpText, IEnumerable<CompositeCommandParameterInfo> parameter, Type returnType, string returnTypeHelp, bool isVisible, bool isEnabled, object presentationData, object presentationLabelData)
        {
            CommandName = commandName;
            HelpText = helpText;
            _parameters = parameter.ToList();
            ReturnType = returnType.FullName;
            ReturnTypeHelp = returnTypeHelp;
            IsVisible = isVisible;
            IsEnabled = isEnabled;
            PresentationData = presentationData;
            PresentationLabelData = presentationLabelData;
        }

        [DataMember]
        public string HelpText { get; }

        [DataMember]
        public string CommandName { get; }

        internal List<CompositeCommandParameterInfo> _parameters;
        [DataMember]
        public IEnumerable<CompositeCommandParameterInfo> Parameters
        {
            get { return _parameters; }
        }

        [DataMember]
        public string ReturnType { get; }

        [DataMember]
        public string ReturnTypeHelp { get; }
        
        [DataMember]
        public bool IsVisible { get; internal set; }

        [DataMember]
        public bool IsEnabled { get; internal set; }
        
        [DataMember]
        public object PresentationData { get; }

        [DataMember]
        public object PresentationLabelData { get; }


    }
}