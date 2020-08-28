using CTLite;
using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts;
using CTLiteDemo.Presentation.Properties;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts
{
    [DataContract]
    [ParentProperty(nameof(PostCompositeContainer.Blog))]
    [CompositeContainer(nameof(PostCompositeContainer.Posts), nameof(Model.BlogApplications.Blogs.Blog.Posts), nameof(PostCompositeContainer.posts))]
    public class PostCompositeContainer : Composite
    {
        public override CompositeState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BlogComposite Blog { get; private set; }

        internal PostCompositeContainer(BlogComposite blogComposite)
        {
            this.InitializeCompositeContainer(out posts, blogComposite);
            _newPostFunc = () => Blog.BlogModel.CreateNewPost();
        }

        private readonly Func<Post> _newPostFunc;

        [NonSerialized]
        internal CompositeDictionary<long, PostComposite> posts;
        [DataMember]
        [Help(typeof(Resources), nameof(Resources.PostCompositeContainer_PostsHelp))]
        public ReadOnlyCompositeDictionary<long, PostComposite> Posts { get; private set; }

        [Command]
        [Help(typeof(Resources), nameof(Resources.PostCompositeContainer_CreateNewPostHelp))]
        [return: Help(typeof(Resources), nameof(Resources.PostCompositeContainer_CreateNewPost_ReturnValueHelp))]
        public PostComposite CreateNewPost(
            [Help(typeof(Resources), nameof(Resources.PostCompositeContainer_CreateNewPost_TitleHelp))] string title,
            [Help(typeof(Resources), nameof(Resources.PostCompositeContainer_CreateNewPost_TextHelp))] string text)
        {
            var newPost = new PostComposite(Blog.BlogModel.CreateNewPost(), this)
            {
                Title = title,
                Text = text,
                State = CompositeState.New
            };

            posts.Add(newPost.Id, newPost);
            return newPost;
        }

        [Command]
        [Help(typeof(Resources), nameof(Resources.PostCompositeContainer_LoadPostsHelp))]
        public void LoadPosts()
        {
            var blogApplication = CompositeRoot as BlogApplicationCompositeRoot;
            var repository = blogApplication.GetService<IMicrosoftSqlServerRepository>();

            using var connection = repository.OpenConnection(blogApplication.BlogDbConnectionString);
            posts.AddRange(repository.Load(connection, null,
                @"
                        SELECT * 
                        FROM Post 
                        WHERE BlogId = @BlogId
                    ",
                new SqlParameter[] { new SqlParameter("@BlogId", Blog.Id) },
                _newPostFunc)
                .Select(p => new PostComposite(p, this)));
        }

        [Command]
        [Help(typeof(Resources), nameof(Resources.PostCompositeContainer_LoadPostsPagedHelp))]
        public void LoadPosts(
            [Help(typeof(Resources), nameof(Resources.PostCompositeContainer_LoadPostsPaged_PageStartHelp))] int pageStart,
            [Help(typeof(Resources), nameof(Resources.PostCompositeContainer_LoadPostsPaged_PageEndHelp))] int pageEnd)
        {
            var blogApplication = CompositeRoot as BlogApplicationCompositeRoot;
            var repository = blogApplication.GetService<IMicrosoftSqlServerRepository>();

            using var connection = repository.OpenConnection(blogApplication.BlogDbConnectionString);
            posts.AddRange(repository.Load(connection, null,

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
                },
                _newPostFunc)
                .Select(p => new PostComposite(p, this)));
        }
    }
}
