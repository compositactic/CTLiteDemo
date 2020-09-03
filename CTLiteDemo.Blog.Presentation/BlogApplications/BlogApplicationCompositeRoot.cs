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
