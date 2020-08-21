using CTLiteDemo.Presentation.BlogApplications;
using CTLite.Data.MicrosoftSqlServer;
using Microsoft.Extensions.Caching.Memory;
using CTLite.AspNetCore;

namespace CTLiteDemo.WebApi
{
    public class BlogApplicationController : CompositeRootControllerBase<BlogApplicationCompositeRoot>
    {
        public BlogApplicationController(IMemoryCache cache) : base(cache) { }
        protected override BlogApplicationCompositeRoot CreateCompositeRoot()
        {
            return new BlogApplicationCompositeRoot
            (
                MicrosoftSqlServerRepository.Create()
            );
        }
    }
}
