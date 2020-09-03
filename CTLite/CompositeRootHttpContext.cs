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
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CTLite
{
    [DataContract]
    public class CompositeRootHttpContext
    {
        [DataMember]
        public CompositeRootHttpRequestContext Request { get; }

        [DataMember]
        public CompositeRootHttpResponseContext Response { get; }

        public CompositeRootHttpContext(string requestContentType,
                                                long requestContentLength64,
                                                Encoding requestContentEncoding,
                                                string httpMethod,
                                                IDictionary<string, string> queryString,
                                                IEnumerable<Cookie> requestCookies,
                                                IDictionary<string, string> requestHeaders,
                                                IEnumerable<string> acceptTypes,
                                                bool hasEntityBody,
                                                bool isAuthenticated,
                                                bool isLocal,
                                                bool isSecureConnection,
                                                bool isWebSocketRequest,
                                                bool requestKeepAlive,
                                                IPEndPoint localEndPoint,
                                                Version requestProtocolVersion,
                                                IPEndPoint remoteEndPoint,
                                                Guid requestTraceIdentifier,
                                                string serviceName,
                                                Uri url,
                                                Uri urlReferrer,
                                                string userAgent,
                                                string userHostAddress,
                                                string userHostName,
                                                IEnumerable<CompositeUploadedFile> uploadedFiles,
                                                X509Certificate2 clientCertificate,
                                                int clientCertificateError,
                                                IEnumerable<string> userLanguages)
        {
            Request = new CompositeRootHttpRequestContext
            {
                acceptTypes = acceptTypes?.ToList(),
                ClientCertificate = clientCertificate,
                ClientCertificateError = clientCertificateError,
                ContentEncoding = requestContentEncoding,
                ContentLength64 = requestContentLength64,
                ContentType = requestContentType,
                HasEntityBody = hasEntityBody,
                HttpMethod = httpMethod,
                IsAuthenticated = isAuthenticated,
                IsLocal = isLocal,
                IsSecureConnection = isSecureConnection,
                IsWebSocketRequest = isWebSocketRequest,
                KeepAlive = requestKeepAlive,
                LocalEndPoint = localEndPoint,
                ProtocolVersion = requestProtocolVersion,
                RemoteEndPoint = remoteEndPoint,
                TraceIdentifier = requestTraceIdentifier,
                ServiceName = serviceName,
                Url = url,
                UrlReferrer = urlReferrer,
                UserAgent = userAgent,
                UserHostAddress = userHostAddress,
                UserHostName = userHostName,
                userLanguages = userLanguages?.ToList(),
                cookies = requestCookies?.ToList(),
                headers = new Dictionary<string, string>(requestHeaders),
                queryString = new Dictionary<string, string>(queryString),
                uploadedFiles = uploadedFiles?.ToList()
            };

            Response = new CompositeRootHttpResponseContext
            {
                headers = new Dictionary<string, string>(),
                cookies = new List<Cookie>()
            };
        }

        internal CompositeRootHttpContext(HttpListenerContext context, IEnumerable<CompositeUploadedFile> uploadedFiles)
        {
            if (context == null)
                return;

            var request = context.Request;

            Request = new CompositeRootHttpRequestContext
            {
                acceptTypes = request.AcceptTypes?.ToList(),
                ClientCertificate = request.GetClientCertificate(),
                ClientCertificateError = request.ClientCertificateError,
                ContentEncoding = request.ContentEncoding,
                ContentLength64 = request.ContentLength64,
                ContentType = request.ContentType,
                HasEntityBody = request.HasEntityBody,
                HttpMethod = request.HttpMethod,
                IsAuthenticated = request.IsAuthenticated,
                IsLocal = request.IsLocal,
                IsSecureConnection = request.IsSecureConnection,
                IsWebSocketRequest = request.IsWebSocketRequest,
                KeepAlive = request.KeepAlive,
                LocalEndPoint = request.LocalEndPoint,
                ProtocolVersion = request.ProtocolVersion,
                RemoteEndPoint = request.RemoteEndPoint,
                TraceIdentifier = request.RequestTraceIdentifier,
                ServiceName = request.ServiceName,
                Url = request.Url,
                UrlReferrer = request.UrlReferrer,
                UserAgent = request.UserAgent,
                UserHostAddress = request.UserHostAddress,
                UserHostName = request.UserHostName,
                userLanguages = request.UserLanguages?.ToList(),
                cookies = PopulateCookies(request),
                headers = PopulateHeaders(request),
                queryString = PopulateQueryString(request),
                uploadedFiles = uploadedFiles?.ToList(),
            };

            Response = new CompositeRootHttpResponseContext
            {
                headers = new Dictionary<string, string>(),
                cookies = new List<Cookie>()
            };
        }

        private static List<Cookie> PopulateCookies(HttpListenerRequest request)
        {
            var cookies = new List<Cookie>();
            foreach (var cookie in request.Cookies)
                cookies.Add(cookie as Cookie);

            return cookies;
        }

        private static Dictionary<string, string> PopulateHeaders(HttpListenerRequest request)
        {
            var headers = new Dictionary<string, string>();
            foreach (var headerName in request.Headers.AllKeys)
                headers.Add(headerName, request.Headers[headerName]);

            return headers;
        }

        private static Dictionary<string, string> PopulateQueryString(HttpListenerRequest request)
        {
            var queryString = new Dictionary<string, string>();
            foreach (var name in request.QueryString.AllKeys)
                queryString.Add(name ?? "value", name == null ? request.QueryString[0] : request.QueryString[name]);

            return queryString;
        }
    }
}