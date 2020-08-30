using CTLite;
using CTLiteDemo.Presentation.BlogApplications;
using CTLiteDemo.WebApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CTLiteDemo.Test
{
    [TestClass]
    public class CompositeRootControllerBaseTests
    {
        private static BlogApplicationController CreateController()
        {
            var blogApplicationController = new BlogApplicationController(new MockMemoryCache())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() { },
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerName = nameof(BlogApplicationController).Replace("Controller", string.Empty)
                    }
                }
            };

            var connection = blogApplicationController.ControllerContext.HttpContext.Connection;
            connection.LocalIpAddress = IPAddress.Loopback;
            connection.RemoteIpAddress = IPAddress.Loopback;
            
            var request = blogApplicationController.ControllerContext.HttpContext.Request;
            request.Scheme = "https";
            request.Host = new HostString(Environment.MachineName);
            request.Protocol = "HTTP/1.1";

            return blogApplicationController;
        }

        private static TResponse SendRequest<TResponse>(string path, string query, string contentType)
        {
            var blogApplicationController = CreateController();

            var request = blogApplicationController.ControllerContext.HttpContext.Request;
            request.Path = new PathString($"/{nameof(BlogApplicationController).Replace("Controller", string.Empty)}{path}");

            if (contentType.ToLower() == "application/x-www-form-urlencoded")
            {
                var requestBodyBytes = Encoding.UTF8.GetBytes(query);
                request.ContentType = contentType;
                request.ContentLength = requestBodyBytes.Length;
                request.Body = new MemoryStream(requestBodyBytes);
            }
            else
            {
                request.Path += query;
                request.ContentLength = 0;
            }

            return (TResponse)blogApplicationController.ReceiveRequest();
        }

        private static IEnumerable<CompositeRootCommandResponse> SendMultiRequest(long sessionId, string multiRequestJson)
        {
            var blogApplicationController = CreateController();
            var request = blogApplicationController.ControllerContext.HttpContext.Request;
            
            request.Path = new PathString($"/{nameof(BlogApplicationController).Replace("Controller", string.Empty)}/{sessionId}");

            var requestBodyBytes = Encoding.UTF8.GetBytes(multiRequestJson);
            request.ContentType = "application/json";
            request.ContentLength = requestBodyBytes.Length;
            request.Body = new MemoryStream(requestBodyBytes);

            return (IEnumerable<CompositeRootCommandResponse>)blogApplicationController.ReceiveRequest();
        }

        private long GetNewSessionId()
        {
            return ((BlogApplicationCompositeRoot)SendRequest<IEnumerable<CompositeRootCommandResponse>>(string.Empty, string.Empty, string.Empty).First().ReturnValue).Id;
        }


        [TestMethod]
        public void DoesCTLiteWorkAndNothingIsBroken()
        {

            var sessionId = GetNewSessionId();
            Assert.IsTrue(sessionId != 0);
            
            var createDatabaseResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/CreateDatabase",
                string.Empty,
                string.Empty
            );
            Assert.IsTrue(createDatabaseResponse.First().Success);

            var setupDatabaseResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/SetupDatabase",
                string.Empty,
                string.Empty
            );
            Assert.IsTrue(setupDatabaseResponse.First().Success);

            var createNewBlogResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/Blogs/CreateNewBlog",
                "?name=Test%20Blog&isActive=true&publishDate=02/02/2002&blogType=Personal&rating=1&earnings=123.45",
                string.Empty
            );
            Assert.IsTrue(createNewBlogResponse.First().Success);

            var createNewPostResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/Blogs/Blogs/0/Posts/CreateNewPost",
                "?title=First Post&text=This is a test post",
                string.Empty
            );
            Assert.IsTrue(createNewPostResponse.First().Success);

            var createNewCommentResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/Blogs/Blogs/0/Posts/Posts/0/Comments/CreateNewComment",
                "?title=First Post&text=This is a test post",
                "application/x-www-form-urlencoded"
            );
            Assert.IsTrue(createNewCommentResponse.First().Success);

            var multiCommandResponse = SendMultiRequest(sessionId, File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "TestMultiCommand.json")));
            Assert.IsTrue(multiCommandResponse.All(cr => cr.Success));

        }
    }
}
