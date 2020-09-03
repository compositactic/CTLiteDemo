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
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Comments
{
    [DataContract]
    [KeyProperty(nameof(CommentComposite.Id), nameof(CommentComposite.OriginalId))]
    [ParentProperty(nameof(CommentComposite.Comments))]
    [CompositeModel(nameof(CommentComposite.CommentModel))]
    public class CommentComposite : Composite
    {
        public override CompositeState State { get => CommentModel.State; set => CommentModel.State = value; }

        internal Comment CommentModel;

        public CommentCompositeContainer Comments { get; private set; }

        internal CommentComposite(Comment comment, CommentCompositeContainer commentCompositeContainer)
        {
            CommentModel = comment;
            Comments = commentCompositeContainer;
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.CommentComposite_IdHelp))]
        public long Id
        {
            get { return CommentModel.Id; }
        }

        public long OriginalId { get { return CommentModel.OriginalId; } }

        [Command]
        [Help(typeof(Resources), nameof(Resources.CommentComposite_RemoveHelp))]
        public void Remove()
        {
            Comments.comments.Remove(Id, true);
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
            }
        }
    }
}
