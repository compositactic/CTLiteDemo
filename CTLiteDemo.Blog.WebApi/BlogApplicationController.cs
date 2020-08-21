using CTLiteDemo.Presentation.BlogApplications;
using CTLite.Data.MicrosoftSqlServer;

namespace CTLiteDemo.WebApi
{
    public class BlogApplicationController : CompositeRootControllerBase<BlogApplicationCompositeRoot>
    {
        protected override BlogApplicationCompositeRoot CreateCompositeRoot()
        {
            return new BlogApplicationCompositeRoot
            (
                MicrosoftSqlServerRepository.Create()
            );
        }
    }
}
