using CTLite;
using CTLiteDemo.Presentation.BlogApplications;
using CTLiteDemo.Presentation.BlogApplications.Blogs;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments;
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
using CTLiteDemo.Model.BlogApplications.Blogs;

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
                    HttpContext = new DefaultHttpContext(),
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

        private static TResponse SendFileUploadRequest<TResponse>(string path, string query, FileInfo[] files)
        {
            var blogApplicationController = CreateController();
            var request = blogApplicationController.ControllerContext.HttpContext.Request;

            request.Path = new PathString($"/{nameof(BlogApplicationController).Replace("Controller", string.Empty)}{path}");
            request.QueryString = new QueryString($"{query}");

            var boundary = $"{Guid.NewGuid()}";
            request.ContentType = $"multipart/form-data; boundary={boundary}";

            var requestBodyStream = new MemoryStream();

            for(int fileIndex = 0; fileIndex < files.Length; fileIndex++)
            {
                var seperatorBytes = Encoding.ASCII.GetBytes($"{(fileIndex == 0 ? string.Empty : Environment.NewLine)}--{boundary}{Environment.NewLine}Content-Disposition: form-data; name=\"filename\"; filename=\"{Path.GetFileName(files[fileIndex].FullName)}\"{Environment.NewLine}Content-Type: {ContentTypes.GetContentTypeFromFileExtension(files[fileIndex].Extension)}{Environment.NewLine}{Environment.NewLine}");
                requestBodyStream.Write(seperatorBytes);
                requestBodyStream.Write(File.ReadAllBytes(files[fileIndex].FullName));
            }

            requestBodyStream.Write(Encoding.ASCII.GetBytes($"{Environment.NewLine}--{boundary}--"));

            requestBodyStream.Position = 0;
            request.ContentLength = requestBodyStream.Length;
            request.Body = requestBodyStream;

            return (TResponse)blogApplicationController.ReceiveRequest();
        }

        private long GetNewSessionId()
        {
            return ((BlogApplicationCompositeRoot)SendRequest<IEnumerable<CompositeRootCommandResponse>>(string.Empty, string.Empty, string.Empty).First().ReturnValue).Id;
        }


        [TestMethod]
        public void CanExecuteAllControllerBaseFunctions()
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
            var newBlog = createNewBlogResponse.First().ReturnValue as BlogComposite;
            Assert.IsTrue(newBlog.Name == "Test Blog" &&
                            newBlog.IsActive &&
                            newBlog.PublishDate == new DateTime(2002, 2, 2) &&
                            newBlog.BlogType == BlogType.Personal &&
                            newBlog.Rating == 1 &&
                            newBlog.Earnings == 123.45m);

            var changePropertyValueResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/Blogs/Blogs/[{newBlog.Id}]/Name",
                "?New Blog Name",
                string.Empty
            );
            Assert.IsTrue(changePropertyValueResponse.First().Success);

            var getPropertyValueResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/Blogs/Blogs/[{newBlog.Id}]/Name",
                string.Empty,
                string.Empty
            );
            Assert.IsTrue(getPropertyValueResponse.First().Success && (string)getPropertyValueResponse.First().ReturnValue == "New Blog Name");

            changePropertyValueResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/Blogs/Blogs/[{newBlog.Id}]/Name",
                "?%00",
                string.Empty
            );
            Assert.IsTrue(changePropertyValueResponse.First().Success);

            getPropertyValueResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/Blogs/Blogs/[{newBlog.Id}]/Name",
                string.Empty,
                string.Empty
            );
            Assert.IsTrue(getPropertyValueResponse.First().Success && (string)getPropertyValueResponse.First().ReturnValue == string.Empty);

            changePropertyValueResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/Blogs/Blogs/[{newBlog.Id}]/Name",
                "?",
                string.Empty
            );
            Assert.IsTrue(changePropertyValueResponse.First().Success);

            getPropertyValueResponse = SendRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/Blogs/Blogs/[{newBlog.Id}]/Name",
                string.Empty,
                string.Empty
            );
            Assert.IsTrue(getPropertyValueResponse.First().Success && (string)getPropertyValueResponse.First().ReturnValue == null);


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

            var createNewAttachmentsResponse = SendFileUploadRequest<IEnumerable<CompositeRootCommandResponse>>
            (
                $"/{sessionId}/Blogs/Blogs/0/Posts/Posts/0/Attachments/CreateNewAttachments",
                "?shouldArchiveAttachments=true",
                new FileInfo[]
                {
                    new FileInfo(Path.Combine(Environment.CurrentDirectory, "doc1.pdf")),
                    new FileInfo(Path.Combine(Environment.CurrentDirectory, "img1.jpg"))
                }
            );
            Assert.IsTrue(createNewAttachmentsResponse.First().Success);

            foreach(var uploadedAttachment in createNewAttachmentsResponse.First().ReturnValue as AttachmentComposite[])
            {
                var getAttachmentResponse = SendRequest<FileContentResult>
                (
                    $"/{sessionId}/Blogs/Blogs/0/Posts/Posts/0/Attachments/Attachments/[{uploadedAttachment.Id}]/GetAttachment",
                    string.Empty,
                    string.Empty
                );

                var originalFileBytes = File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory, Path.GetFileName(uploadedAttachment.FilePath)));
                Assert.IsTrue(Enumerable.SequenceEqual(originalFileBytes, getAttachmentResponse.FileContents));
            }
        }
    }
}
