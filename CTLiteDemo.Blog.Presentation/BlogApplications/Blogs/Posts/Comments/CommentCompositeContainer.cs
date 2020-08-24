using CTLite;
using CTLiteDemo.Presentation.Properties;
using System;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Comments
{
    [DataContract]
    [ParentProperty(nameof(CommentCompositeContainer.Post))]
    [CompositeContainer(nameof(CommentCompositeContainer.Comments), nameof(Model.BlogApplications.Blogs.Posts.Post.Comments))]
    public class CommentCompositeContainer : Composite
    {
        public override CompositeState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PostComposite Post { get; private set; }
        internal CommentCompositeContainer(PostComposite postComposite)
        {
            this.InitializeCompositeContainer(out comments, postComposite);
        }

        [NonSerialized]
        internal CompositeDictionary<long, CommentComposite> comments;
        [DataMember]
        [Help(typeof(Resources), nameof(Resources.CommentsCompositeContainer_CommentsHelp))]
        public ReadOnlyCompositeDictionary<long, CommentComposite> Comments { get; private set; }

        [Command]
        [Help(typeof(Resources), nameof(Resources.CommentCompositeContainer_CreateNewCommentHelp))]
        [return: Help(typeof(Resources), nameof(Resources.CommentCompositeContainer_CreateNewComment_ReturnValueHelp))]
        public CommentComposite CreateNewComment(
            [Help(typeof(Resources), nameof(Resources.CommentCompositeContainer_CreateNewComment_TextHelp))] string text)
        {
            var newComment = new CommentComposite(Post.PostModel.CreateNewComment(), this)
            {
                Text = text,
                State = CompositeState.New
            };

            comments.Add(newComment.Id, newComment);
            return newComment;
        }
    }
}
