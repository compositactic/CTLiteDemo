using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace CTLite
{
    [DataContract]
    public class CompositeRootHttpResponseContext
    {
        internal CompositeRootHttpResponseContext() { }

        public Encoding ContentEncoding { get; set; }

        [DataMember]
        public long? ContentLength64 { get; set; }

        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        internal List<Cookie> cookies;
        public Cookie[] GetCookies() { return cookies.ToArray(); }

        public void AddCookie(Cookie cookie)
        {
            cookies.Add(cookie);
        }

        [DataMember]
        internal Dictionary<string, string> headers;
        public KeyValuePair<string, string>[] GetHeaders()
        {
            return headers.ToArray();
        }

        public void AddHeader(string name, string value)
        {
            headers.Add(name, value);
        }

        [DataMember]
        public bool? KeepAlive { get; set; }

        public Version ProtocolVersion { get; set; }

        [DataMember]
        public string RedirectLocation { get; set; }

        [DataMember]
        public bool? SendChunked { get; set; }

        [DataMember]
        public int? StatusCode { get; set; }

        [DataMember]
        public string StatusDescription { get; set; }
    }
}