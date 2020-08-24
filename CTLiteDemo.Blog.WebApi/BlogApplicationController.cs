using CTLite.AspNetCore;
using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Presentation.BlogApplications;
using CTLiteDemo.Service.BlogApplications.Blogs.Posts.Attachments;
using Microsoft.Extensions.Caching.Memory;

namespace CTLiteDemo.WebApi
{
    public class BlogApplicationController : CompositeRootControllerBase<BlogApplicationCompositeRoot>
    {
        public BlogApplicationController(IMemoryCache cache) : base(cache) { }
        protected override BlogApplicationCompositeRoot CreateCompositeRoot()
        {
            return new BlogApplicationCompositeRoot
            (
                MicrosoftSqlServerRepository.Create(),
                new AttachmentArchiveService()
            );
        }
    }
}
