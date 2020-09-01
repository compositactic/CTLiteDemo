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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

[assembly: InternalsVisibleTo("CTLiteDemo.Test")]

namespace CTLiteDemo.Presentation.BlogApplications
{
    [DataContract]
    [CompositeModel(nameof(BlogApplicationCompositeRoot.BlogApplicationModel))]
    public class BlogApplicationCompositeRoot : CompositeRoot
    {
        internal BlogApplication BlogApplicationModel;

        public override CompositeState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public BlogApplicationCompositeRoot() : base()
        {
            Initialize();
        }

        public BlogApplicationCompositeRoot(params IService[] services) : base(services)
        {
            Initialize();
        }

        public override long Id => BlogApplicationModel.Id;

        private void Initialize()
        {
            BlogApplicationModel = new BlogApplication();
            Blogs = new BlogCompositeContainer(this);
        }

        public override void InitializeCompositeModel(object model)
        {
            BlogApplicationModel = model as BlogApplication;
            Blogs = new BlogCompositeContainer(this);
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogApplicationCompositeRoot_BlogsHelp))]
        public BlogCompositeContainer Blogs { get; private set; }

        private string _blogDbConnectionString;
        internal string BlogDbConnectionString
        {
            get
            {
                if(string.IsNullOrEmpty(_blogDbConnectionString))
                {
                    var applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var blogApplicationSettings = GetSettings(applicationPath);
                    var sqlConnectionString = blogApplicationSettings.TryGetValue($"{Environment.MachineName}.ConnectionString", out string connectionString) ? connectionString : blogApplicationSettings["Local.ConnectionString"];
                    _blogDbConnectionString = string.Format(sqlConnectionString, blogApplicationSettings.TryGetValue($"{Environment.MachineName}.Database.BlogDb", out string blogDbConnString) ? blogDbConnString : blogApplicationSettings["Local.Database.BlogDb"]);
                }

                return _blogDbConnectionString;
            }
        }

        private string _masterDbConnectionString;
        internal string MasterDbConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_masterDbConnectionString))
                {
                    var applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var blogApplicationSettings = GetSettings(applicationPath);
                    var sqlConnectionString = blogApplicationSettings.TryGetValue($"{Environment.MachineName}.ConnectionString", out string connectionString) ? connectionString : blogApplicationSettings["Local.ConnectionString"];
                    _masterDbConnectionString = string.Format(sqlConnectionString, blogApplicationSettings.TryGetValue($"{Environment.MachineName}.Database.Master", out string masterDbConnString) ? masterDbConnString : blogApplicationSettings["Local.Database.Master"]);
                }

                return _masterDbConnectionString;
            }
        }

        internal void SetConnectionStrings(string masterDbConnectionString, string blogDbConnectionString)
        {
            _masterDbConnectionString = masterDbConnectionString;
            _blogDbConnectionString = blogDbConnectionString;
        }

        public static Dictionary<string, string> GetSettings(string applicationPath)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(applicationPath, "BlogApplicationSettings.json")));
        }

        [Command]
        public void CreateDatabase()
        {
            var applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var createDatabaseSqlScriptFile = Path.Combine(applicationPath, "000-BlogDatabase.sql");

            var repository = GetService<IMicrosoftSqlServerRepository>();

            using var connection = repository.OpenConnection(MasterDbConnectionString);
            var createDatabaseSql = File.ReadAllText(createDatabaseSqlScriptFile);
            repository.Execute<object>(connection, null, createDatabaseSql, null);
        }

        internal void CreateDatabase(string createDatabaseSql)
        {
            var repository = GetService<IMicrosoftSqlServerRepository>();
            using var connection = repository.OpenConnection(MasterDbConnectionString);
            repository.Execute<object>(connection, null, createDatabaseSql, null);
        }

        [Command]
        public void SetupDatabase()
        {
            var repository = GetService<IMicrosoftSqlServerRepository>();
            var applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using (var connection = repository.OpenConnection(BlogDbConnectionString))
            using (var transaction = repository.BeginTransaction(connection))
            {
                repository.CreateHelperStoredProcedures(connection, transaction);
                repository.CommitTransaction(transaction);
            }

            using (var connection = repository.OpenConnection(BlogDbConnectionString))
            using (var transaction = repository.BeginTransaction(connection))
            {
                var directories = 
                    Directory.GetDirectories(Path.Combine(applicationPath, "BlogApplications"), string.Empty, SearchOption.AllDirectories)
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
