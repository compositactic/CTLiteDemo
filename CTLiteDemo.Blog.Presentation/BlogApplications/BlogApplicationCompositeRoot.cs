using CTLite;
using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Model.BlogApplications;
using CTLiteDemo.Presentation.BlogApplications.Blogs;
using CTLiteDemo.Presentation.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications
{
    [DataContract]
    [CompositeModel(nameof(BlogApplicationCompositeRoot.BlogApplicationModel))]
    public class BlogApplicationCompositeRoot : CompositeRoot
    {
        internal BlogApplication BlogApplicationModel;

        public BlogApplicationCompositeRoot(BlogApplication blogApplication) : base()
        {
            Initialize(blogApplication);
        }

        public BlogApplicationCompositeRoot(BlogApplication blogApplication, params IService[] services) : base(services)
        {
            Initialize(blogApplication);
        }

        public BlogApplicationCompositeRoot(BlogApplication blogApplication, IEnumerable<Assembly> serviceAssemblies) : base(serviceAssemblies)
        {
            Initialize(blogApplication);
        }

        private void Initialize(BlogApplication blogApplication)
        {
            if (blogApplication == null)
            {
                BlogApplicationModel = new BlogApplication();

                var machineName = Environment.MachineName;
                var blogApplicationSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "BlogApplicationSettings.json")));

                BlogApplicationModel.ConnectionString = blogApplicationSettings.TryGetValue($"{machineName}.MsSqlConnectionString", out string connectionString) ?
                                    connectionString :
                                    blogApplicationSettings["Local.MsSqlConnectionString"];

                BlogApplicationModel.MasterDbConnectionString = string.Format(ConnectionString, blogApplicationSettings.TryGetValue($"{machineName}.Database.Master", out string masterDbConnectionString) ? masterDbConnectionString : blogApplicationSettings["Local.Database.Master"]);
                BlogApplicationModel.BlogDbConnectionString = string.Format(ConnectionString, blogApplicationSettings.TryGetValue($"{machineName}.Database.BlogDb", out string blogDbConnectionString) ? blogDbConnectionString : blogApplicationSettings["Local.Database.BlogDb"]);

            }
            else
                BlogApplicationModel = blogApplication;


            AllBlogs = new BlogCompositeContainer(this);
        }

        internal string BlogDbConnectionString { get { return BlogApplicationModel.BlogDbConnectionString; } }
        internal string MasterDbConnectionString { get { return BlogApplicationModel.MasterDbConnectionString; } }
        internal string ConnectionString { get { return BlogApplicationModel.ConnectionString; } }


        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogApplicationCompositeRoot_AllBlogs))]
        public BlogCompositeContainer AllBlogs { get; private set; }

        private string _errorMessage;
        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogApplicationCompositeRoot_ErrorMessage))]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                NotifyPropertyChanged(nameof(ErrorMessage));
            }
        }

        [Command]
        public void SetupDatabase()
        {
            var repository = GetService<IMicrosoftSqlServerRepository>();

            var createDatabaseSqlScriptFile = Path.Combine(Environment.CurrentDirectory, "000-BlogServerDatabase.sql");

            using (var connection = repository.OpenConnection(MasterDbConnectionString))
            {
                var createDatabaseSql = File.ReadAllText(createDatabaseSqlScriptFile);
                repository.Execute<object>(connection, null, createDatabaseSql, null);
            }

            using (var connection = repository.OpenConnection(BlogDbConnectionString))
            using (var transaction = repository.BeginTransaction(connection))
            {
                repository.CreateHelperStoredProcedures(connection, transaction);
                repository.CommitTransaction(transaction);
            }

            using (var connection = repository.OpenConnection(BlogDbConnectionString))
            using (var transaction = repository.BeginTransaction(connection))
            {
                var directories = Directory
                    .GetDirectories(Path.Combine(Environment.CurrentDirectory, "BlogApplications"), string.Empty, SearchOption.AllDirectories)
                    .GroupBy(d => new { Depth = d.Split(Path.DirectorySeparatorChar).Count(), Directory = d })
                    .OrderBy(g => g.Key.Depth).ThenBy(g => g.Key.Directory)
                    .Select(g => g.Key.Directory);

                foreach (var directory in directories)
                {
                    foreach (var sqlScriptFile in Directory.GetFiles(directory, "*.sql"))
                    {
                        var script = File.ReadAllText(sqlScriptFile);
                        repository.Execute<object>(connection, transaction, script, null);
                    }
                }

                repository.CommitTransaction(transaction);
            }
        }
    }
}
