using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts.Comments;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Comments
{
    [DataContract]
    [KeyProperty(nameof(CommentComposite.Id))]
    [ParentProperty(nameof(CommentComposite.AllComments))]
    [CompositeModel(nameof(CommentComposite.CommentModel))]
    public class CommentComposite : Composite
    {
        internal CommentComposite(Comment comment, CommentCompositeContainer commentCompositeContainer)
        {
            CommentModel = comment;
            AllComments = commentCompositeContainer;
        }


        public CommentCompositeContainer AllComments { get; private set; }

        public Comment CommentModel { get; }

        [DataMember]
        public long Id
        {
            get { return CommentModel.Id; }
        }


        [DataMember]
        public string Text
        {
            get { return CommentModel.Text; }
            set
            {
                CommentModel.Text = value;
                NotifyPropertyChanged(nameof(Text));
            }
        }

        [Command]
        public void Remove()
        {
            AllComments.comments.Remove(Id);
        }
    }
}
