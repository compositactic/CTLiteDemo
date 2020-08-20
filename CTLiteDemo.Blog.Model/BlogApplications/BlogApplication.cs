using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace CTLiteDemo.Model.BlogApplications
{
    [DataContract]
    [KeyProperty(nameof(BlogApplication.Id))]
    public class BlogApplication
    {
        [DataMember]
        public long Id { get; set; }

        public BlogApplication()
        {
            Id = new long().NewId();
            blogs = new ConcurrentDictionary<long, Blog>();
            _blogs = new ReadOnlyDictionary<long, Blog>(blogs);
        }

        [DataMember]
        internal ConcurrentDictionary<long, Blog> blogs;
        private ReadOnlyDictionary<long, Blog> _blogs;
        public IReadOnlyDictionary<long, Blog> Blogs
        {
            get { return _blogs; }
        }

        [DataMember]
        public string BlogDbConnectionString { get; set; }
        [DataMember]
        public string MasterDbConnectionString { get; set; }
        [DataMember]
        public string ConnectionString { get; set; }

        public Blog CreateNewBlog()
        {
            return new Blog(this);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _blogs = new ReadOnlyDictionary<long, Blog>(blogs);
        }
    }
}
