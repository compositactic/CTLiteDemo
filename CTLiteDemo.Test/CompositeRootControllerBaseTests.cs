using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Model.BlogApplications.Blogs;
using CTLiteDemo.Presentation.BlogApplications;
using CTLiteDemo.Service.BlogApplications.Blogs.Posts.Attachments;
using CTLiteDemo.WebApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using CTLite;
using System.Linq;

namespace CTLiteDemo.Test
{
    [TestClass]
    public class CompositeRootControllerBaseTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var blogApplicationCompositeRoot = new BlogApplicationCompositeRoot
            (
                MicrosoftSqlServerRepository.Create(),
                new AttachmentArchiveService()
            );

            var newBlog = blogApplicationCompositeRoot.Blogs.CreateNewBlog("Test Blog", true, DateTime.Now, BlogType.Public, 1, 123.45m);

            newBlog.Save();
        }

        [TestMethod]
        public void TestMethod2()
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
            request.Path = new PathString($"/{nameof(BlogApplicationController).Replace("Controller", string.Empty)}");
            request.ContentLength = 0;
            request.Scheme = "https";
            request.Host = new HostString(Environment.MachineName);

            //request.Path = new PathString("/BlogApplication/637343168600464556/Blogs/CreateNewBlog");
            //request.QueryString = new QueryString("?name=Justin%20Blog&isActive=true&publishDate=02/02/2002&blogType=Personal&rating=1&earnings=123.45");

            //request.ContentLength = 0;
            //request.ContentType = "";
            //request.Body = new MemoryStream();

            var ret = (BlogApplicationCompositeRoot)((IEnumerable<CompositeRootCommandResponse>)blogApplicationController.ReceiveRequest()).First().ReturnValue;

        }
    }
}
