using System;
using System.Runtime.Serialization;

namespace CTLite
{
    [DataContract]
    public class CompositeRootCommandRequest
    {
        private CompositeRootCommandRequest() { }

        public static CompositeRootCommandRequest Create(int id, string command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return new CompositeRootCommandRequest(id) { CommandPath = command };
        }

        internal CompositeRootCommandRequest(int id)
        {
            Id = id;
        }

        [DataMember]
        public string CommandPath { get; internal set; }

        [DataMember]
        public int Id { get; private set; }
    }
}