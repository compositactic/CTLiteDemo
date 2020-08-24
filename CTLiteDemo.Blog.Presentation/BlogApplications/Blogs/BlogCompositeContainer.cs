using CTLite;
using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Model.BlogApplications.Blogs;
using CTLiteDemo.Presentation.Properties;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs
{
    [DataContract]
    [ParentProperty(nameof(BlogCompositeContainer.BlogApplication))]
    [CompositeContainer(nameof(BlogCompositeContainer.Blogs), nameof(Model.BlogApplications.BlogApplication.Blogs))]
    public class BlogCompositeContainer : Composite
    {
        public override CompositeState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BlogApplicationCompositeRoot BlogApplication { get; private set; }

        internal BlogCompositeContainer(BlogApplicationCompositeRoot blogApplicationCompositeRoot)
        {
            this.InitializeCompositeContainer(out blogs, blogApplicationCompositeRoot);
        }

        [NonSerialized]
        internal CompositeDictionary<long, BlogComposite> blogs;
        [DataMember]
        public ReadOnlyCompositeDictionary<long, BlogComposite> Blogs { get; private set; }

        [Command]
        [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlogHelp))]
        [return: Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_ReturnValueHelp))]
        public BlogComposite CreateNewBlog(
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_NameHelp))] string name,
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_IsActiveHelp))] bool isActive,
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_PublishDateHelp))] DateTime publishDate,
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_BlogTypeHelp))] BlogType blogType,
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_RatingHelp))] int? rating,
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_EarningsHelp))] decimal earnings)
        {
            var newBlog = new BlogComposite(BlogApplication.BlogApplicationModel.CreateNewBlog(), this)
            {
                Name = name,
                IsActive = isActive,
                PublishDate = publishDate,
                BlogType = blogType,
                Rating = rating,
                Earnings = earnings,
                State = CompositeState.New
            };

            blogs.Add(newBlog.Id, newBlog);

            return newBlog;
        }

        [Command]
        [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_LoadBlogHelp))]
        public void LoadBlog(
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_LoadBlog_NameHelp))] string name)
        {
            var blogApplication = CompositeRoot as BlogApplicationCompositeRoot;
            var repository = blogApplication.GetService<IMicrosoftSqlServerRepository>();

            using var connection = repository.OpenConnection(blogApplication.BlogDbConnectionString);
            blogs.AddRange(repository.Load<Blog>(connection, null,
                @"
                        SELECT * 
                        FROM Blog 
                        WHERE Name = @Name
                ",
                new SqlParameter[] { new SqlParameter("@Name", name) })
                .Select(blog => new BlogComposite(blog, this)));
        }
    }
}
