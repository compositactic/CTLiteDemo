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
