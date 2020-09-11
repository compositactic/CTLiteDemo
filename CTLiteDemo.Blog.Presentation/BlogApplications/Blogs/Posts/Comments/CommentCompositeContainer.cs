// CTLiteDemo - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
// NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts.Comments;
using CTLiteDemo.Presentation.Properties;
using System;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Comments
{
    [DataContract]
    [ParentProperty(nameof(CommentCompositeContainer.Post))]
    [CompositeContainer(nameof(CommentCompositeContainer.Comments), nameof(CommentCompositeContainer.Comments), nameof(CommentCompositeContainer.comments))]
    public class CommentCompositeContainer : Composite
    {
        public override CompositeState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PostComposite Post { get; private set; }
        internal CommentCompositeContainer(PostComposite postComposite)
        {
            this.InitializeCompositeContainer(out comments, postComposite);
            _newCommentFunc = () => Post.PostModel.CreateNewComment();
        }

        private readonly Func<Comment> _newCommentFunc;

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
            var newComment = new CommentComposite(_newCommentFunc.Invoke(), this)
            {
                Text = text,
                State = CompositeState.New
            };

            comments.Add(newComment.Id, newComment);
            return newComment;
        }
    }
}
