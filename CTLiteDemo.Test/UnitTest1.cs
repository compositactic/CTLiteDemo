using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Model.BlogApplications.Blogs;
using CTLiteDemo.Presentation.BlogApplications;
using CTLiteDemo.Service.BlogApplications.Blogs.Posts.Attachments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CTLiteDemo.Test
{
    [TestClass]
    public class UnitTest1
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
    }
}
