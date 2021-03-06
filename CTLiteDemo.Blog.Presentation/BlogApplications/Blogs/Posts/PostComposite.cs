﻿// CTLiteDemo - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

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
using CTLiteDemo.Model.BlogApplications.Blogs.Posts;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Comments;
using CTLiteDemo.Presentation.Properties;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts
{
    [DataContract]
    [KeyProperty(nameof(PostComposite.Id), nameof(PostComposite.OriginalId))]
    [ParentProperty(nameof(PostComposite.Posts))]
    [CompositeModel(nameof(PostComposite.PostModel))]
    public class PostComposite : Composite
    {
        public override CompositeState State { get => PostModel.State; set => PostModel.State = value; }

        internal Post PostModel;

        public PostCompositeContainer Posts { get; }

        internal PostComposite(Post post, PostCompositeContainer postCompositeContainer)
        {
            PostModel = post;
            Posts = postCompositeContainer;

            Comments = new CommentCompositeContainer(this);
            Attachments = new AttachmentCompositeContainer(this);
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.PostComposite_IdHelp))]
        public long Id
        {
            get { return PostModel.Id; }
        }

        public long OriginalId { get { return PostModel.OriginalId; } }

        [Command]
        [Help(typeof(Resources), nameof(Resources.PostComposite_RemoveHelp))]
        public void Remove()
        {
            Posts.posts.Remove(Id, true);
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.PostComposite_CommentsHelp))]
        public CommentCompositeContainer Comments { get; }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.PostComposite_AttachmentsHelp))]
        public AttachmentCompositeContainer Attachments { get; }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.PostComposite_TitleHelp))]
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
        [Help(typeof(Resources), nameof(Resources.PostComposite_TextHelp))]
        public string Text
        {
            get { return PostModel.Text; }
            set
            {
                PostModel.Text = value;
                NotifyPropertyChanged(nameof(PostComposite.Text));
            }
        }
    }
}
