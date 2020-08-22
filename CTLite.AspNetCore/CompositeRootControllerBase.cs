using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CTLite.AspNetCore
{
    [ApiController]
    [Route("[controller]")]
    public abstract class CompositeRootControllerBase<TCompositeRoot> : ControllerBase where TCompositeRoot : CompositeRoot, new()
    {
        private readonly IMemoryCache _cache;
        public CompositeRootControllerBase(IMemoryCache cache)
        {
            _cache = cache;
        }

        protected virtual void SetCache(long id, string jsonValue)
        {
            _cache.Set(id, jsonValue);
        }

        protected virtual string GetCache(long id)
        {
            return _cache.Get(id) as string;
        }

        protected virtual TCompositeRoot CreateCompositeRoot()
        {
            return new TCompositeRoot();
        }

        [HttpGet]
        [HttpPost]
        [Route("{**catchAll}")]
        public object ReceiveRequest()
        {
            var commandResponses = new List<CompositeRootCommandResponse>();

            try
            {
                var request = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
                IEnumerable<CompositeUploadedFile> uploadedFiles = null;
                IEnumerable<CompositeRootCommandRequest> commandRequests = null;

                var requestParts = request.Split('?');
                string requestBody = requestParts.Length >= 3 ? requestParts[2] : null;
                if (requestBody == null && Request.ContentLength.HasValue)
                    requestBody = Request.Body.GetRequest(Encoding.UTF8, Request.ContentType, string.Empty, CultureInfo.CurrentCulture, out uploadedFiles, out commandRequests);

                commandRequests ??= new CompositeRootCommandRequest[]
                {
                    CompositeRootCommandRequest.Create(1, Regex.Replace($"{Request.Path.Value}{Request.QueryString.Value}", $"^/{ControllerContext.ActionDescriptor.ControllerName}/", string.Empty ))
                };

                var compositeRootHttpContext = GetContext(requestBody, uploadedFiles);

                var compositeRoot = CreateCompositeRoot();
                var compositeRootModelFieldName = compositeRoot.GetType().GetCustomAttribute<CompositeModelAttribute>()?.ModelFieldName;
                var compositeRootModelField = compositeRoot.GetType().GetField(compositeRootModelFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                var compositeRootModelFieldType = compositeRootModelField.FieldType;

                var compositeRootModelJson = GetCache(compositeRoot.Id);
                if (!string.IsNullOrWhiteSpace(compositeRootModelJson))
                    compositeRootModelField.SetValue(compositeRoot, JsonConvert.DeserializeObject(compositeRootModelJson, compositeRootModelFieldType));
                else
                    SetCache(compositeRoot.Id, JsonConvert.SerializeObject(compositeRootModelField.GetValue(compositeRoot)));

                commandResponses = compositeRoot.Execute(commandRequests, compositeRootHttpContext, uploadedFiles).ToList();
                SetCache(compositeRoot.Id, JsonConvert.SerializeObject(compositeRootModelField.GetValue(compositeRoot)));

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
            catch(Exception e)
            {
                commandResponses.Add(new CompositeRootCommandResponse { Success = false, Errors = GetErrorMessages(e) });
                return commandResponses;
            }
        }

        private static IEnumerable<string> GetErrorMessages(Exception e)
        {
            return GetErrorMessages(e, new List<string>());
        }

        private static IEnumerable<string> GetErrorMessages(Exception e, List<string> messages)
        {
            if (e == null)
                return messages;

            messages.Add(e.Message);
            return GetErrorMessages(e.InnerException, messages);
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
