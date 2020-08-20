using CTLite;
using System;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Comments
{
    [DataContract]
    [ParentProperty(nameof(CommentCompositeContainer.Post))]
    [CompositeContainer(nameof(CommentCompositeContainer.Comments), nameof(Model.BlogApplications.Blogs.Posts.Post.Comments))]
    public class CommentCompositeContainer : Composite
    {
        public PostComposite Post { get; private set; }
        internal CommentCompositeContainer(PostComposite postComposite)
        {
            this.InitializeCompositeContainer(out comments, postComposite);
        }

        [NonSerialized]
        internal CompositeDictionary<long, CommentComposite> comments;
        [DataMember]
        public ReadOnlyCompositeDictionary<long, CommentComposite> Comments { get; private set; }

        [Command]
        public CommentComposite CreateNewComment()
        {
            var newComment = new CommentComposite(Post.PostModel.CreateNewComment(), this)
            {
                State = CompositeState.New
            };

            comments.Add(newComment.Id, newComment);
            return newComment;
        }
    }
}
