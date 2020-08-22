using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CTLite
{
    public class CompositeRootCommandResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public IEnumerable<string> Errors { get; set; }

        [DataMember]
        public object ReturnValue { get; set; }

        [DataMember]
        public string ReturnValueContentType { get; set; }

        [DataMember]
        public string ReturnValueContentEncoding { get; set; }

        [DataMember]
        public int Id { get; internal set; }
    }
}