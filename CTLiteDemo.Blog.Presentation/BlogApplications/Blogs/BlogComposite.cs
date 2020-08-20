using CTLite;
using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Model.BlogApplications.Blogs;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs
{
    [DataContract]
    [KeyProperty(nameof(BlogComposite.Id))]
    [ParentProperty(nameof(BlogComposite.AllBlogs))]
    [CompositeModel(nameof(BlogModel))]
    public class BlogComposite : Composite
    {
        public BlogCompositeContainer AllBlogs { get; }

        internal Blog BlogModel;

        internal BlogComposite(Blog blog, BlogCompositeContainer blogCompositeContainer)
        {
            BlogModel = blog;
            AllBlogs = blogCompositeContainer;
            AllPosts = new PostCompositeContainer(this);
        }

        [DataMember]
        public PostCompositeContainer AllPosts { get; }

        [DataMember]
        public long Id
        {
            get { return BlogModel.Id; }
        }

        [DataMember]
        public string Name
        {
            get { return BlogModel.Name; }
            set
            {
                BlogModel.Name = value;
                NotifyPropertyChanged(nameof(BlogComposite.Name));
            }
        }

        [Command]
        public void Remove()
        {
            AllBlogs.blogs.Remove(Id);
        }

        [Command]
        public void Save()
        {
            var blogApplication = CompositeRoot as BlogApplicationCompositeRoot;
            var repository = blogApplication.GetService<IMicrosoftSqlServerRepository>();

            using var connection = repository.OpenConnection(blogApplication.BlogDbConnectionString);
            using var transaction = repository.BeginTransaction(connection);
            repository.Save(connection, transaction, this);
            transaction.Commit();
        }
    }
}
