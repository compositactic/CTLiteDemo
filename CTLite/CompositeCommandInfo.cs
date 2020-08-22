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
        internal CompositeCommandInfo(string commandName, string helpText, IEnumerable<CompositeCommandParameterInfo> parameter, Type returnType, string returnTypeHelp)
        {
            CommandName = commandName;
            HelpText = helpText;
            _parameters = parameter.ToList();
            ReturnType = returnType.FullName;
            ReturnTypeHelp = returnTypeHelp;
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
    }
}