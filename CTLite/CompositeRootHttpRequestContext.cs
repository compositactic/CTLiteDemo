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
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CTLite
{
    [DataContract]
    public class CompositeRootHttpRequestContext
    {
        internal CompositeRootHttpRequestContext() { }

        [DataMember]
        public string SessionToken { get; internal set; }

        [DataMember]
        internal List<string> acceptTypes;

        public IEnumerable<string> AcceptTypes { get { return acceptTypes; } }

        [DataMember]
        public int ClientCertificateError { get; internal set; }

        public Encoding ContentEncoding { get; internal set; }

        [DataMember]
        public long ContentLength64 { get; internal set; }

        [DataMember]
        public string ContentType { get; internal set; }

        [DataMember]
        internal List<Cookie> cookies;

        public IEnumerable<Cookie> Cookies { get { return cookies; } }

        [DataMember]
        public bool HasEntityBody { get; internal set; }

        [DataMember]
        internal Dictionary<string, string> headers;

        public IReadOnlyDictionary<string, string> Headers { get { return headers; } }

        [DataMember]
        public string HttpMethod { get; internal set; }

        [DataMember]
        public bool IsAuthenticated { get; internal set; }

        [DataMember]
        public bool IsLocal { get; internal set; }

        [DataMember]
        public bool IsSecureConnection { get; internal set; }

        [DataMember]
        public bool IsWebSocketRequest { get; internal set; }

        [DataMember]
        public bool KeepAlive { get; internal set; }

        public IPEndPoint LocalEndPoint { get; internal set; }
        public Version ProtocolVersion { get; internal set; }

        [DataMember]
        internal Dictionary<string, string> queryString;

        public IReadOnlyDictionary<string, string> QueryString { get { return queryString; } }
        public IPEndPoint RemoteEndPoint { get; internal set; }

        [DataMember]
        public Guid TraceIdentifier { get; internal set; }

        [DataMember]
        public string ServiceName { get; internal set; }

        [DataMember]
        public Uri Url { get; internal set; }

        [DataMember]
        public Uri UrlReferrer { get; internal set; }

        [DataMember]
        public string UserAgent { get; internal set; }

        [DataMember]
        public string UserHostAddress { get; internal set; }

        [DataMember]
        public string UserHostName { get; internal set; }

        [DataMember]
        internal List<string> userLanguages;

        public IEnumerable<string> UserLanguages { get { return userLanguages; } }

        [DataMember]
        internal List<CompositeUploadedFile> uploadedFiles;

        public IEnumerable<CompositeUploadedFile> UploadedFiles { get { return uploadedFiles; } }

        public X509Certificate2 ClientCertificate { get; internal set; }
    }
}