using CTLite;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace CTLiteDemo.WebApi
{
    [ApiController]
    [Route("[controller]")]
    public abstract class CompositeRootControllerBase<TCompositeRoot> : ControllerBase where TCompositeRoot : CompositeRoot, new()
    {
        public CompositeRootControllerBase()
        {

        }

        protected virtual void SetCache(string sessionToken, string jsonValue)
        {
            //var memoryCache = MemoryCache.Default;
            //memoryCache.Set(sessionToken, jsonValue, DateTime.Now.Add(TimeSpan.FromMinutes(20)));
        }

        protected virtual string GetCache(string sessionToken)
        {
            //var memoryCache = MemoryCache.Default;
            //return memoryCache.Get(sessionToken) as string;
            return string.Empty;
        }

        [HttpGet]
        [HttpPost]
        public object ReceiveRequest([FromForm] object _)
        {
            var request = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
            IEnumerable<CompositeUploadedFile> uploadedFiles = null;
            IEnumerable<CompositeRootCommandRequest> commandRequests = null;

            var requestParts = request.Split('?');
            string requestBody = requestParts.Length >= 3 ? requestParts[2] : null;
            if (requestBody == null && Request.ContentLength.HasValue)
                requestBody = Request.Body.GetRequest(Encoding.UTF8, Request.ContentType, string.Empty, CultureInfo.CurrentCulture, out uploadedFiles, out commandRequests);

            var compositeRootHttpContext = GetContext(requestBody, uploadedFiles);

            var compositeRoot = new TCompositeRoot();
            var compositeRootModelFieldName = compositeRoot.GetType().GetCustomAttribute<CompositeModelAttribute>()?.ModelFieldName;
            var compositeRootModelField = compositeRoot.GetType().GetField(compositeRootModelFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var compositeRootModelFieldType = compositeRootModelField.FieldType;

            var sessionToken = requestParts[1].Split('/')[0];

            var compositeRootModelJson = GetCache(sessionToken);
            compositeRootModelField.SetValue(compositeRoot, JsonConvert.DeserializeObject(compositeRootModelJson, compositeRootModelFieldType));

            var commandResponses = compositeRoot.Execute(commandRequests, compositeRootHttpContext, uploadedFiles);
            SetCache(sessionToken, JsonConvert.SerializeObject(compositeRootModelField.GetValue(compositeRoot)));
            if (commandResponses.First().ReturnValue is byte[])
            {
                Response.ContentType = compositeRootHttpContext.Response.ContentType;
                Response.ContentLength = compositeRootHttpContext.Response.ContentLength64;
                return commandResponses.First().ReturnValue;
            }
            else
            {
                Response.ContentType = "application/json";
                return commandResponses;
            }
         
        }

        private CompositeRootHttpContext GetContext(string requestBody, IEnumerable<CompositeUploadedFile> uploadedFiles)
        {
            var compositeRootHttpContext = new CompositeRootHttpContext
            (
                requestContentType: Request.ContentType,
                requestContentLength64: Request.ContentLength ?? 0,
                requestContentEncoding: Encoding.UTF8,
                httpMethod: Request.Method,
                queryString: Request.ContentLength.HasValue ?
                    new Dictionary<string, string>() :
                        !string.IsNullOrEmpty(requestBody) ?
                        new Dictionary<string, string>(requestBody.Split('&').Select(s => new KeyValuePair<string, string>(s.Split('=')[0], s.Split('=')[1]))) :
                        new Dictionary<string, string>(),
                requestCookies: Request.Cookies.Select(c => new Cookie(c.Key, c.Value)),
                requestHeaders: new Dictionary<string, string>(Request.Headers.Select(q => new KeyValuePair<string, string>(q.Key, q.Value))),
                acceptTypes: Request.Headers["Accept"],
                hasEntityBody: Request.ContentLength.HasValue,
                isAuthenticated: false,
                isLocal: false,
                isSecureConnection: Request.IsHttps,
                isWebSocketRequest: false,
                requestKeepAlive: true,
                localEndPoint: new IPEndPoint(HttpContext.Connection.LocalIpAddress, HttpContext.Connection.LocalPort),
                requestProtocolVersion: null, //new Version(Request.Protocol),
                remoteEndPoint: new IPEndPoint(HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort),
                requestTraceIdentifier: new Guid(),
                serviceName: string.Empty,
                url: new Uri(Request.Scheme + "://" + Request.Host + Request.Path + Request.QueryString),
                urlReferrer: null,
                userAgent: Request.Headers["User-Agent"],
                userHostAddress: string.Empty,
                userHostName: string.Empty,
                uploadedFiles: uploadedFiles,
                clientCertificate: HttpContext.Connection.ClientCertificate,
                clientCertificateError: 0,
                userLanguages: Request.Headers["Accept-Language"]
            );

            return compositeRootHttpContext;
        }
    }
}
