using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Comments;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts
{
    [DataContract]
    [KeyProperty(nameof(PostComposite.Id))]
    [ParentProperty(nameof(PostComposite.AllPosts))]
    [CompositeModel(nameof(PostComposite.PostModel))]
    public class PostComposite : Composite
    {
        internal PostComposite(Post post, PostCompositeContainer postCompositeContainer)
        {
            PostModel = post;
            AllPosts = postCompositeContainer;

            AllComments = new CommentCompositeContainer(this);
            AllAttachments = new AttachmentCompositeContainer(this);
        }

        public PostCompositeContainer AllPosts { get; }

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
        public CommentCompositeContainer AllComments { get; }

        [DataMember]
        public AttachmentCompositeContainer AllAttachments { get; }

        [Command]
        public void Remove()
        {
            AllPosts.posts.Remove(Id);
        }
    }
}
