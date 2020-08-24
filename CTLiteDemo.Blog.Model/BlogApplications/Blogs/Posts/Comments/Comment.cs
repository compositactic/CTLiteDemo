using CTLite;
using System;
using System.Runtime.Serialization;

namespace CTLiteDemo.Model.BlogApplications.Blogs.Posts.Comments
{
    [DataContract]
    [ParentProperty(nameof(Comment.Post))]
    [KeyProperty(nameof(Comment.Id))]
    public class Comment
    {
        [DataMember]
        public CompositeState State { get; set; } = CompositeState.Unchanged;

        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public long PostId { get; set; }
        public Post Post { get; internal set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public long UserId { get; set; }

        public Comment() { }

        internal Comment(Post post)
        {
            PostId = post.Id;

            Post = post ?? throw new ArgumentNullException(nameof(post));
            Post.comments.Load(this, _ => { return new long().NewId(); });
        }

        public void Remove()
        {
            Post.comments.TryRemove(Id, out _);
        }
    }
}
