// CTLite.AspNetCore - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

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

using Microsoft.AspNetCore.Http;
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

        protected virtual void SetCache(long cacheId, string jsonValue)
        {
            _cache.Set(cacheId, jsonValue);
        }

        protected virtual string GetCache(long cacheId)
        {
            return _cache.Get(cacheId) as string;
        }

        protected virtual TCompositeRoot CreateCompositeRoot()
        {
            return new TCompositeRoot();
        }

        protected virtual void Authenticate(HttpContext httpContext) 
        {
            //throw new UnauthorizedAccessException();
        }

        [HttpGet]
        [HttpPost]
        [Route("{**catchAll}")]
        public object ReceiveRequest()
        {
            var commandResponses = new List<CompositeRootCommandResponse>();

            try
            {
                Authenticate(HttpContext);

                var request = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
                IEnumerable<CompositeUploadedFile> uploadedFiles = null;
                IEnumerable<CompositeRootCommandRequest> commandRequests = null;

                var requestParts = request.Split('?');
                string requestBody = requestParts.Length >= 3 ? requestParts[2] : null;
                if (requestBody == null && Request.ContentLength.HasValue)
                    requestBody = Request.Body.GetRequest(Encoding.UTF8, Request.ContentType, string.Empty, CultureInfo.CurrentCulture, out uploadedFiles, out commandRequests);

                var pathAndQuery = $"{Regex.Replace(Request.Path.Value, @"\0$", "%00")}{(string.IsNullOrEmpty(Request.QueryString.Value) && !string.IsNullOrEmpty(requestBody) ? (!requestBody.StartsWith("?") ? "?" : string.Empty) + requestBody : Request.QueryString.Value)}";
                var controllerName = ControllerContext.ActionDescriptor.ControllerName;
                var requestPattern = $"^/{controllerName}/?(?'cacheId'[0-9]+)?/?";
                Match cacheIdMatch;
                var cacheId = 0L;
                if ((cacheIdMatch = Regex.Match(pathAndQuery, requestPattern)).Success) 
                {
                    var cacheIdValue = cacheIdMatch.Groups["cacheId"].Value;
                    cacheId = string.IsNullOrEmpty(cacheIdValue) ? 0 : long.Parse(cacheIdValue);
                }

                commandRequests ??= new CompositeRootCommandRequest[]
                {
                    CompositeRootCommandRequest.Create(1, Regex.Replace(pathAndQuery, requestPattern, string.Empty))
                };

                var compositeRootHttpContext = GetContext(requestBody, uploadedFiles);

                var compositeRoot = CreateCompositeRoot();
                var compositeRootModelFieldName = compositeRoot.GetType().GetCustomAttribute<CompositeModelAttribute>()?.ModelFieldName;
                var compositeRootModelField = compositeRoot.GetType().GetField(compositeRootModelFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                var compositeRootModelFieldType = compositeRootModelField.FieldType;

                var compositeRootModelJson = GetCache(cacheId);
                if (!string.IsNullOrWhiteSpace(compositeRootModelJson))
                {
                    var compositeRootModel = JsonConvert.DeserializeObject(compositeRootModelJson, compositeRootModelFieldType);
                    compositeRootModel.RestoreParentReferences();
                    compositeRoot.InitializeCompositeModel(compositeRootModel);
                }
                else
                {
                    if (cacheId == 0)
                        SetCache(compositeRoot.Id, JsonConvert.SerializeObject(compositeRootModelField.GetValue(compositeRoot)));
                    else
                        throw new UnauthorizedAccessException();
                }

                OnBeforeExecute(commandRequests, compositeRootHttpContext, uploadedFiles);
                commandResponses = compositeRoot.Execute(commandRequests, compositeRootHttpContext, uploadedFiles).ToList();
                OnAfterExecute(commandResponses, compositeRootHttpContext);

                SetCache(compositeRoot.Id, JsonConvert.SerializeObject(compositeRootModelField.GetValue(compositeRoot)));

                if (commandResponses.First().ReturnValue is byte[] bytes)
                {
                    return File
                    (
                        bytes,
                        compositeRootHttpContext.Response.ContentType
                    );
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

        protected virtual void OnAfterExecute(IEnumerable<CompositeRootCommandResponse> commandResponses, CompositeRootHttpContext compositeRootHttpContext)
        {
        }

        protected virtual void OnBeforeExecute(IEnumerable<CompositeRootCommandRequest> commandRequests, CompositeRootHttpContext compositeRootHttpContext, IEnumerable<CompositeUploadedFile> uploadedFiles)
        {
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
                isWebSocketRequest: HttpContext.WebSockets.IsWebSocketRequest,
                requestKeepAlive: true,
                localEndPoint: new IPEndPoint(HttpContext.Connection.LocalIpAddress, HttpContext.Connection.LocalPort),
                requestProtocolVersion: new Version(Regex.Replace(Request.Protocol, "[^0-9.]", string.Empty)),
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
