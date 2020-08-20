using CTLite;
using System;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs
{
    [DataContract]
    [ParentProperty(nameof(BlogCompositeContainer.BlogApplication))]
    [CompositeContainer(nameof(BlogCompositeContainer.Blogs))]
    public class BlogCompositeContainer : Composite
    {
        public BlogApplicationCompositeRoot BlogApplication { get; private set; }

        internal BlogCompositeContainer(BlogApplicationCompositeRoot blogApplicationCompositeRoot)
        {
            this.InitializeCompositeContainer(out blogs, blogApplicationCompositeRoot);
        }

        [NonSerialized]
        internal CompositeDictionary<long, BlogComposite> blogs;
        [DataMember]
        public ReadOnlyCompositeDictionary<long, BlogComposite> Blogs { get; private set; }
    }
}
