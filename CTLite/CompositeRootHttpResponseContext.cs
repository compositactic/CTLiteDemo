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