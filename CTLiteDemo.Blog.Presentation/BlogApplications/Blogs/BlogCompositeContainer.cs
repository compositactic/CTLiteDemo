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
using CTLiteDemo.Model.BlogApplications.Blogs;
using CTLiteDemo.Presentation.Properties;
using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs
{
    [DataContract]
    [Category("Tee")]
    [ParentProperty(nameof(BlogCompositeContainer.BlogApplication))]
    [CompositeContainer(nameof(BlogCompositeContainer.Blogs), nameof(BlogCompositeContainer.Blogs), nameof(BlogCompositeContainer.blogs))]
    public class BlogCompositeContainer : Composite
    {
        public override CompositeState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BlogApplicationCompositeRoot BlogApplication { get; private set; }

        internal BlogCompositeContainer(BlogApplicationCompositeRoot blogApplicationCompositeRoot)
        {
            this.InitializeCompositeContainer(out blogs, blogApplicationCompositeRoot);
            _newBlogFunc = () => BlogApplication.BlogApplicationModel.CreateNewBlog();
        }

        private readonly Func<Blog> _newBlogFunc;

        [NonSerialized]
        internal CompositeDictionary<long, BlogComposite> blogs;
        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_BlogsHelp))]
        public ReadOnlyCompositeDictionary<long, BlogComposite> Blogs { get; private set; }

        [Command]
        [PresentationStateControl(nameof(CanCreateNewBlog), nameof(IsVisible), null, nameof(GetPresentationData), nameof(GetPresentationLabelData))]
        [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlogHelp))]
        [return: Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_ReturnValueHelp))]
        public BlogComposite CreateNewBlog(
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_NameHelp))] string name,
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_IsActiveHelp))] bool isActive,
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_PublishDateHelp))] DateTime publishDate,
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_BlogTypeHelp))] BlogType blogType,
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_RatingHelp))] int? rating,
            [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_CreateNewBlog_EarningsHelp))] decimal earnings)
        {
            var newBlog = new BlogComposite(_newBlogFunc.Invoke(), this)
            {
                Name = name,
                IsActive = isActive,
                PublishDate = publishDate,
                BlogType = blogType,
                Rating = rating,
                Earnings = earnings,
                State = CompositeState.New
            };

            blogs.Add(newBlog.Id, newBlog);

            return newBlog;
        }

        public bool IsVisible()
        {
            return true;
        }

        public bool CanCreateNewBlog()
        {
            return true;
        }

        public object GetPresentationLabelData()
        {
            return new { @class = "class-for-current-label-state", onclick = "alert('hi')" };
        }

        public object GetPresentationData()
        {
            return new { @class = "class-for-current-state", onclick = "alert('hi')" };
        }

        [Command]
        [Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_LoadBlogHelp))]
        public void LoadBlog([Help(typeof(Resources), nameof(Resources.BlogCompositeContainer_LoadBlog_NameHelp))] string name)
        {
            var blogApplication = CompositeRoot as BlogApplicationCompositeRoot;
            var repository = blogApplication.GetService<IMicrosoftSqlServerRepository>();

            using var connection = repository.OpenConnection(blogApplication.BlogDbConnectionString);

            blogs.AddRange(repository.Load(connection, null,
                @"
                        SELECT * 
                        FROM Blog 
                        WHERE Name = @Name
                ",
                new SqlParameter[] { new SqlParameter("@Name", name) },
                _newBlogFunc)
                .Select(blog => new BlogComposite(blog, this)));
        }

        [Command]
        public BlogCompositeContainer SaveAll(bool shouldUpdatedInsertedIds)
        {
            return this.Save(shouldUpdatedInsertedIds);
        }
    }
}
