using CTLite;
using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Presentation.BlogApplications;

namespace CTLiteDemo.Presentation
{
    public static class ExtensionMethods
    {
        public static TComposite Save<TComposite>(this TComposite composite, bool shouldUpdatedInsertedIds)  where TComposite : Composite
        {
            var blogApplication = composite.CompositeRoot as BlogApplicationCompositeRoot;
            var repository = blogApplication.GetService<IMicrosoftSqlServerRepository>();

            using var connection = repository.OpenConnection(blogApplication.BlogDbConnectionString);
            using var transaction = repository.BeginTransaction(connection);
            repository.Save(connection, transaction, composite, shouldUpdatedInsertedIds);
            transaction.Commit();
            return composite;
        }
    }
}
