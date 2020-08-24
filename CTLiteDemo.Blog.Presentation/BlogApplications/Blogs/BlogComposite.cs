﻿using CTLite;
using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Model.BlogApplications.Blogs;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts;
using CTLiteDemo.Presentation.Properties;
using System;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs
{
    [DataContract]
    [KeyProperty(nameof(BlogComposite.Id))]
    [ParentProperty(nameof(BlogComposite.Blogs))]
    [CompositeModel(nameof(BlogModel))]
    public class BlogComposite : Composite
    {
        public BlogCompositeContainer Blogs { get; }

        internal Blog BlogModel;

        internal BlogComposite(Blog blog, BlogCompositeContainer blogCompositeContainer)
        {
            BlogModel = blog;
            Blogs = blogCompositeContainer;
            Posts = new PostCompositeContainer(this);
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_PostsHelp))]
        public PostCompositeContainer Posts { get; }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_IdHelp))]
        public long Id
        {
            get { return BlogModel.Id; }
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_NameHelp))]
        public string Name
        {
            get { return BlogModel.Name; }
            set
            {
                BlogModel.Name = value;
                NotifyPropertyChanged(nameof(BlogComposite.Name));
                State = CompositeState.Modified;
            }
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_IsActiveHelp))]
        public bool IsActive 
        {
            get { return BlogModel.IsActive;  }
            set 
            {
                BlogModel.IsActive = value;
                NotifyPropertyChanged(nameof(BlogComposite.IsActive));
                State = CompositeState.Modified;
            }
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_PublishDateHelp))]
        public DateTime PublishDate
        {
            get { return BlogModel.PublishDate; }
            set
            {
                BlogModel.PublishDate = value;
                NotifyPropertyChanged(nameof(BlogComposite.PublishDate));
                State = CompositeState.Modified;
            }
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_BlogTypeHelp))]
        public BlogType BlogType 
        { 
            get { return BlogModel.BlogType; }
            set
            {
                BlogModel.BlogType = value;
                NotifyPropertyChanged(nameof(BlogComposite.BlogType));
            }
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_RatingHelp))]
        public int? Rating 
        { 
            get { return BlogModel.Rating; }
            set
            {
                BlogModel.Rating = value;
                NotifyPropertyChanged(nameof(BlogComposite.Rating));
                State = CompositeState.Modified;
            }
        }


        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_EarningsHelp))]
        public decimal Earnings 
        { 
            get { return BlogModel.Earnings; }
            set
            {
                BlogModel.Earnings = value;
                NotifyPropertyChanged(nameof(BlogComposite.Earnings));
                State = CompositeState.Modified;
            }
        }


        [Command]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_RemoveHelp))]
        public void Remove()
        {
            Blogs.blogs.Remove(Id);
        }

        [Command]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_SaveHelp))]
        public void Save()
        {
            var blogApplication = CompositeRoot as BlogApplicationCompositeRoot;
            var repository = blogApplication.GetService<IMicrosoftSqlServerRepository>();

            using var connection = repository.OpenConnection(blogApplication.BlogDbConnectionString);
            using var transaction = repository.BeginTransaction(connection);
            repository.Save(connection, transaction, this);
            transaction.Commit();
        }
    }
}
