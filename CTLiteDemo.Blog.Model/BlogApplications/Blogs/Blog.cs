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
using CTLiteDemo.Model.BlogApplications.Blogs.Posts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace CTLiteDemo.Model.BlogApplications.Blogs
{
    [DataContract]
    [ParentProperty(nameof(Blog.BlogApplication))]
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

        public Post CreateNewPost()
        {
            return new Post(this);
        }

        [DataMember]
        internal ConcurrentDictionary<long, Post> posts;
        private ReadOnlyDictionary<long, Post> _posts;
        public IReadOnlyDictionary<long, Post> Posts
        {
            get { return _posts; }
        }

        public void Remove()
        {
            BlogApplication.blogs.TryRemove(Id, out _);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _posts = new ReadOnlyDictionary<long, Post>(posts);
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

        [DataMember]
        [NoDb]
        public string NoDbSampleProperty { get; set; }
    }
}
