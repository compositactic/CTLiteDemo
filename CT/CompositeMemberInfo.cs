using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CTLite
{
    [DataContract]
    [Serializable]
    public class CompositeMemberInfo
    {
        internal CompositeMemberInfo(IEnumerable<CompositePropertyInfo> compositePropertyInfos, IEnumerable<CompositeCommandInfo> compositeCommandInfos)
        {
            Properties = compositePropertyInfos;
            Commands = compositeCommandInfos;
        }

        [DataMember]
        public IEnumerable<CompositePropertyInfo> Properties { get; }

        [DataMember]
        public IEnumerable<CompositeCommandInfo> Commands { get; }
    }
}