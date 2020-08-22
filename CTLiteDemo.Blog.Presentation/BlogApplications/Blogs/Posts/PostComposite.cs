using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Comments;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts
{
    [DataContract]
    [KeyProperty(nameof(PostComposite.Id))]
    [ParentProperty(nameof(PostComposite.Posts))]
    [CompositeModel(nameof(PostComposite.PostModel))]
    public class PostComposite : Composite
    {
        internal PostComposite(Post post, PostCompositeContainer postCompositeContainer)
        {
            PostModel = post;
            Posts = postCompositeContainer;

            Comments = new CommentCompositeContainer(this);
            Attachments = new AttachmentCompositeContainer(this);
        }

        public PostCompositeContainer Posts { get; }

        internal Post PostModel;

        [DataMember]
        public long Id
        {
            get { return PostModel.Id; }
        }

        [DataMember]
        public string Title
        {
            get { return PostModel.Title; }
            set
            {
                PostModel.Title = value;
                NotifyPropertyChanged(nameof(PostComposite.Title));
            }
        }

        [DataMember]
        public string Text
        {
            get { return PostModel.Text; }
            set
            {
                PostModel.Text = value;
                NotifyPropertyChanged(nameof(PostComposite.Text));
            }
        }

        [DataMember]
        public CommentCompositeContainer Comments { get; }

        [DataMember]
        public AttachmentCompositeContainer Attachments { get; }

        [Command]
        public void Remove()
        {
            Posts.posts.Remove(Id);
        }
    }
}
