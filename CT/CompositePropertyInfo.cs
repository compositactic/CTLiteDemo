using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace CTLite
{
    [DataContract]
    [Serializable]
    public class CompositePropertyInfo
    {
        internal CompositePropertyInfo(string propertyName, Type propertyType, bool isReadOnly, string helpText)
        {
            PropertyName = propertyName;

            Match m;
            if ((m = Regex.Match(propertyType.FullName, @"CT.ReadOnlyCompositeDictionary[^[]*\[\[(?'key_type'[^,]+).+?(?=\])],\[(?'value_type'[^,]+)")).Success)
                PropertyType = $"CT.ReadOnlyCompositeDictionary<{m.Groups["key_type"].Value}, {m.Groups["value_type"].Value}>";
            else
                PropertyType = propertyType.FullName;

            PropertyEnumValues = propertyType.GetTypeEnumValues();

            IsReadOnly = isReadOnly;
            HelpText = helpText;
        }

        [DataMember]
        public string HelpText { get; }

        [DataMember]
        public string[] PropertyEnumValues { get; }

        [DataMember]
        public string PropertyName { get; }

        [DataMember]
        public string PropertyType { get; }

        [DataMember]
        public bool IsReadOnly { get; }
    }
}