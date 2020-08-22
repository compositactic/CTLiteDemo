using System;
using System.Runtime.Serialization;

namespace CTLite
{
    [DataContract]
    [Serializable]
    public class CompositeCommandParameterInfo
    {
        internal CompositeCommandParameterInfo(string parameterName, Type parameterType, string helpText, string[] parameterEnumValues)
        {
            ParameterName = parameterName;
            _parameterType = parameterType;
            HelpText = helpText;
            ParameterEnumValues = parameterEnumValues;
        }

        [DataMember]
        public string ParameterName { get; }

        [DataMember]
        public string HelpText { get; }

        private readonly Type _parameterType;
        [DataMember]
        public string ParameterType { get { return _parameterType.FullName; } }

        [DataMember]
        public string[] ParameterEnumValues { get; }
    }
}