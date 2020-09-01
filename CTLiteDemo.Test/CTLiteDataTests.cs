using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Model.BlogApplications.Blogs;
using CTLiteDemo.Presentation.BlogApplications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
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

            for(int b = 0; b < 1000; b++)
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
        }
    }
}
