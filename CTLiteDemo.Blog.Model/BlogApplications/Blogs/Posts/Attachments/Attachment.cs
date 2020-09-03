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
using System;
using System.Runtime.Serialization;

namespace CTLiteDemo.Model.BlogApplications.Blogs.Posts.Attachments
{
    [DataContract]
    [ParentProperty(nameof(Attachment.Post))]
    [KeyProperty(nameof(Attachment.Id), nameof(OriginalId))]
    public class Attachment
    {
        [DataMember]
        public CompositeState State { get; set; } = CompositeState.Unchanged;

        [DataMember]
        public long Id { get; set; }

        public long OriginalId { get; set; }

        [DataMember]
        public long PostId { get; set; }
        public Post Post { get; internal set; }

        [DataMember]
        public string FilePath { get; set; }

        public Attachment() { }

        internal Attachment(Post post)
        {
            PostId = post.Id;

            Post = post ?? throw new ArgumentNullException(nameof(post));
            Post.attachments.Load(this, _ => { return new long().NewId(); });
        }

        public void Remove()
        {
            Post.attachments.TryRemove(Id, out _);
        }
    }
}
