using CTLite;
using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts
{
    [DataContract]
    [ParentProperty(nameof(PostCompositeContainer.Blog))]
    [CompositeContainer(nameof(PostCompositeContainer.Posts), nameof(Model.BlogApplications.Blogs.Blog.Posts))]
    public class PostCompositeContainer : Composite
    {
        public BlogComposite Blog { get; private set; }

        internal PostCompositeContainer(BlogComposite blogComposite)
        {
            this.InitializeCompositeContainer(out posts, blogComposite);
        }

        [NonSerialized]
        internal CompositeDictionary<long, PostComposite> posts;
        [DataMember]
        public ReadOnlyCompositeDictionary<long, PostComposite> Posts { get; private set; }

        [Command]
        public PostComposite CreateNewPost()
        {
            var newPost = new PostComposite(Blog.BlogModel.CreateNewPost(), this)
            {
                State = CompositeState.New
            };

            posts.Add(newPost.Id, newPost);
            return newPost;
        }

        [Command]
        public void LoadPosts()
        {
            var blogApplication = CompositeRoot as BlogApplicationCompositeRoot;
            var repository = blogApplication.GetService<IMicrosoftSqlServerRepository>();

            using var connection = repository.OpenConnection(blogApplication.BlogDbConnectionString);
            posts.AddRange(repository.Load<Post>(connection, null,
                @"
                        SELECT * 
                        FROM Post 
                        WHERE BlogId = @BlogId
                    ",
                new SqlParameter[] { new SqlParameter("@BlogId", Blog.Id) })
                .Select(p => new PostComposite(p, this)));
        }

        [Command]
        public void LoadPosts(int pageStart, int pageEnd)
        {
            var blogApplication = CompositeRoot as BlogApplicationCompositeRoot;
            var repository = blogApplication.GetService<IMicrosoftSqlServerRepository>();

            using var connection = repository.OpenConnection(blogApplication.BlogDbConnectionString);
            posts.AddRange(repository.Load<Post>(connection, null,

                @"
                      WITH Posts AS
                      (
                        SELECT ROW_NUMBER() OVER(ORDER BY ID DESC) AS RowNumber, *
                        FROM Post 
                        WHERE BlogId = @BlogId
                      )
                      SELECT *
                      FROM Posts
                      WHERE RowNumber BETWEEN @pageStart AND @pageEnd
                 ",

                new SqlParameter[]
                {
                        new SqlParameter("@BlogId", Blog.Id),
                        new SqlParameter("@pageStart", pageStart),
                        new SqlParameter("@pageEnd", pageEnd)
                })
                .Select(p => new PostComposite(p, this)));
        }
    }
}
