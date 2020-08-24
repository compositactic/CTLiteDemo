using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Comments;
using CTLiteDemo.Presentation.Properties;
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
        [Help(typeof(Resources), nameof(Resources.PostComposite_IdHelp))]
        public long Id
        {
            get { return PostModel.Id; }
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.PostComposite_TitleHelp))]
        public string Title
        {
            get { return PostModel.Title; }
            set
            {
                PostModel.Title = value;
                NotifyPropertyChanged(nameof(PostComposite.Title));
                State = CompositeState.Modified;
            }
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.PostComposite_TextHelp))]
        public string Text
        {
            get { return PostModel.Text; }
            set
            {
                PostModel.Text = value;
                NotifyPropertyChanged(nameof(PostComposite.Text));
                State = CompositeState.Modified;
            }
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.PostComposite_CommentsHelp))]
        public CommentCompositeContainer Comments { get; }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.PostComposite_AttachmentsHelp))]
        public AttachmentCompositeContainer Attachments { get; }

        [Command]
        [Help(typeof(Resources), nameof(Resources.PostComposite_RemoveHelp))]
        public void Remove()
        {
            Posts.posts.Remove(Id);
        }
    }
}
