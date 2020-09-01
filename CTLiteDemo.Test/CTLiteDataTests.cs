using CTLite;
using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Model.BlogApplications.Blogs;
using CTLiteDemo.Presentation.BlogApplications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CTLiteDemo.Test
{
    [TestClass]
    public class CTLiteDataTests
    {
        private readonly string _masterDbConnectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=SSPI;";

        [TestMethod]
        public void CanPerformAllCTDataFunctions()
        {
            var blogApplicationCompositeRoot = new BlogApplicationCompositeRoot
            (
                MicrosoftSqlServerRepository.Create()
            );
            
            blogApplicationCompositeRoot.SetConnectionStrings
            (
                _masterDbConnectionString,
                "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=BlogDb2;Integrated Security=SSPI;"
            );

            var applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var createDatabaseSqlScriptFile = Path.Combine(applicationPath, "000-BlogDatabase2.sql");
            var createDatabaseSql = File.ReadAllText(createDatabaseSqlScriptFile);

            blogApplicationCompositeRoot.CreateDatabase(createDatabaseSql);
            blogApplicationCompositeRoot.SetupDatabase();

            var random = new Random();

            for(int b = 0; b < 100; b++)
            {
                var r = random.Next(-1000, 1000);
                var newBlog = blogApplicationCompositeRoot.Blogs.CreateNewBlog
                (
                    $"Blog Name {r}",
                    r < 0,
                    DateTime.Now.AddDays(r),
                    r > 0 ? BlogType.Personal : BlogType.Public,
                    r,
                    (decimal)new Random().NextDouble() * r
                );

                for(int p = 0; p < 10; p++)
                {
                    r = random.Next(-1000, 1000);
                    var newPost = newBlog.Posts.CreateNewPost
                    (
                        $"Post {r}",
                        $"Text post {r}"
                    );

                    for (int c = 0; c < 5; c++)
                    {
                        r = random.Next(-1000, 1000);
                        newPost.Comments.CreateNewComment
                        (
                            $"Comment {r}"
                        );
                    }
                }
            }

            blogApplicationCompositeRoot.Blogs.SaveAll();
            Assert.IsTrue(blogApplicationCompositeRoot.Blogs.Blogs.Values.All
            (
                b => b.Id != 0 && 
                b.Id != b.OriginalId &&
                b.State == CompositeState.Unchanged
            ));

            Assert.IsTrue(!blogApplicationCompositeRoot.Blogs.Blogs.Values.Any(b => b.Name == "CT Blog"));
            blogApplicationCompositeRoot.Blogs.LoadBlog("CT Blog");
            Assert.IsTrue(blogApplicationCompositeRoot.Blogs.Blogs.Values.Any(b => b.Name == "CT Blog" && b.State == CompositeState.Unchanged));

            var ctBlog = blogApplicationCompositeRoot.Blogs.Blogs.Values.Single(b => b.Name == "CT Blog");
            ctBlog.PublishDate = DateTime.Now;
            ctBlog.Rating = 1;
            ctBlog.BlogType = BlogType.Public;
            ctBlog.Earnings = 0;

            foreach (var blog in blogApplicationCompositeRoot.Blogs.Blogs.Values)
                blog.BlogType = BlogType.Personal;

            blogApplicationCompositeRoot.Blogs.SaveAll();

            var repository = blogApplicationCompositeRoot.GetService<IMicrosoftSqlServerRepository>();
            using var connection = repository.OpenConnection(blogApplicationCompositeRoot.BlogDbConnectionString);
            var updatedBlogs = repository.Load(connection, null, @"SELECT * FROM Blog", null, () => new Blog());
            Assert.IsTrue(updatedBlogs.All(b => b.BlogType == BlogType.Personal));

            blogApplicationCompositeRoot.Blogs.Blogs.Values.Single(b => b.Name == "CT Blog").Remove();
            blogApplicationCompositeRoot.Blogs.SaveAll();
            var removedBlogsShouldBeEmpty = repository.Load(connection, null, @"SELECT * FROM Blog WHERE Name = 'CT Blog'", null, () => new Blog());
            Assert.IsTrue(removedBlogsShouldBeEmpty.Count() == 0);
        }
    }
}
