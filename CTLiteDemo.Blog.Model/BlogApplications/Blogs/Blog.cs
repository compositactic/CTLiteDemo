using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace CTLiteDemo.Model.BlogApplications.Blogs
{
    [DataContract]
    [ParentProperty(nameof(Blog.BlogApplicationId))]
    [KeyProperty(nameof(Blog.Id), nameof(Blog.OriginalId))]
    public class Blog
    {

        [DataMember]
        public CompositeState State { get; set; } = CompositeState.Unchanged;

        [DataMember]
        public long Id { get; set; }

        public long OriginalId { get; set; }

        public long BlogApplicationId { get; set; }

        public BlogApplication BlogApplication { get; internal set; }

        public Blog() 
        {
            posts = new ConcurrentDictionary<long, Post>();
            _posts = new ReadOnlyDictionary<long, Post>(posts);
        }

        internal Blog(BlogApplication blogApplication)
        {
            BlogApplicationId = blogApplication.Id;
            BlogApplication = blogApplication ?? throw new ArgumentNullException(nameof(blogApplication));
            BlogApplication.blogs.Load(this, _ => { return new long().NewId(); });

            posts = new ConcurrentDictionary<long, Post>();
            _posts = new ReadOnlyDictionary<long, Post>(posts);
        }

        [DataMember]
        internal ConcurrentDictionary<long, Post> posts;
        private ReadOnlyDictionary<long, Post> _posts;
        public IReadOnlyDictionary<long, Post> Posts
        {
            get { return _posts; }
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public DateTime PublishDate { get; set; }

        [DataMember]
        public BlogType BlogType { get; set; }

        [DataMember]
        public int? Rating { get; set; }

        [DataMember]
        public decimal Earnings { get; set; }

        public Post CreateNewPost()
        {
            return new Post(this);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _posts = new ReadOnlyDictionary<long, Post>(posts);
        }

        public void Remove()
        {
            BlogApplication.blogs.TryRemove(Id, out _);
        }
    }
}
