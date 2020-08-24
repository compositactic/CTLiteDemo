using CTLite;
using CTLiteDemo.Model.Properties;

namespace CTLiteDemo.Model.BlogApplications.Blogs
{
    public enum BlogType
    {
        [Help(typeof(Resources), nameof(Resources.BlogType_Public))]
        Public,

        [Help(typeof(Resources), nameof(Resources.BlogType_Personal))]
        Personal
    }
}
