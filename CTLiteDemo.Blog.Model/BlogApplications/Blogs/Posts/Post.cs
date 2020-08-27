using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts.Attachments;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts.Comments;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace CTLiteDemo.Model.BlogApplications.Blogs.Posts
{
    [DataContract]
    [ParentProperty(nameof(Post.Blog))]
    [KeyProperty(nameof(Post.Id), nameof(Post.OriginalId))]
    public class Post
    {
        [DataMember]
        public CompositeState State { get; set; } = CompositeState.Unchanged;

        public Post() { }

        [DataMember]
        public long Id { get; set; }

        public long OriginalId { get; set; }

        [DataMember]
        public long BlogId { get; set; }

        public Blog Blog { get; internal set; }

        internal Post(Blog blog)
        {
            BlogId = blog.Id;

            comments = new ConcurrentDictionary<long, Comment>();
            _comments = new ReadOnlyDictionary<long, Comment>(comments);

            attachments = new ConcurrentDictionary<long, Attachment>();
            _attachments = new ReadOnlyDictionary<long, Attachment>(attachments);

            Blog = blog ?? throw new ArgumentNullException(nameof(blog));
            Blog.posts.Load(this, _ => { return new long().NewId(); });
        }

        public void Remove()
        {
            Blog.posts.TryRemove(Id, out _);
        }

        public Comment CreateNewComment()
        {
            return new Comment(this);
        }

        [DataMember]
        internal ConcurrentDictionary<long, Comment> comments;
        private ReadOnlyDictionary<long, Comment> _comments;
        public IReadOnlyDictionary<long, Comment> Comments
        {
            get { return _comments; }
        }

        public Attachment CreateNewAttachment()
        {
            return new Attachment(this);
        }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        internal ConcurrentDictionary<long, Attachment> attachments;
        private ReadOnlyDictionary<long, Attachment> _attachments;
        public IReadOnlyDictionary<long, Attachment> Attachments
        {
            get { return _attachments; }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _comments = new ReadOnlyDictionary<long, Comment>(comments);
            _attachments = new ReadOnlyDictionary<long, Attachment>(attachments);
        }
    }
}
