using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts.Comments;
using CTLiteDemo.Presentation.Properties;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Comments
{
    [DataContract]
    [KeyProperty(nameof(CommentComposite.Id))]
    [ParentProperty(nameof(CommentComposite.Comments))]
    [CompositeModel(nameof(CommentComposite.CommentModel))]
    public class CommentComposite : Composite
    {
        internal CommentComposite(Comment comment, CommentCompositeContainer commentCompositeContainer)
        {
            CommentModel = comment;
            Comments = commentCompositeContainer;
        }

        public CommentCompositeContainer Comments { get; private set; }

        public Comment CommentModel { get; }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.CommentComposite_IdHelp))]
        public long Id
        {
            get { return CommentModel.Id; }
        }


        [DataMember]
        [Help(typeof(Resources), nameof(Resources.CommentComposite_TextHelp))]
        public string Text
        {
            get { return CommentModel.Text; }
            set
            {
                CommentModel.Text = value;
                NotifyPropertyChanged(nameof(Text));
                State = CompositeState.Modified;
            }
        }

        [Command]
        [Help(typeof(Resources), nameof(Resources.CommentComposite_RemoveHelp))]
        public void Remove()
        {
            Comments.comments.Remove(Id);
        }
    }
}
