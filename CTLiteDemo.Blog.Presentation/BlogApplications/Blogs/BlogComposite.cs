﻿using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts;
using CTLiteDemo.Presentation.Properties;
using System;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs
{
    [DataContract]
    [KeyProperty(nameof(BlogComposite.Id), nameof(BlogComposite.OriginalId))]
    [ParentProperty(nameof(BlogComposite.Blogs))]
    [CompositeModel(nameof(BlogModel))]
    public class BlogComposite : Composite
    {
        public override CompositeState State { get => BlogModel.State; set => BlogModel.State = value; }
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

        public long OriginalId { get { return BlogModel.OriginalId; } }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_NameHelp))]
        public string Name
        {
            get { return BlogModel.Name; }
            set
            {
                BlogModel.Name = value;
                NotifyPropertyChanged(nameof(BlogComposite.Name));
                State = State == CompositeState.New ? CompositeState.New : CompositeState.Modified;
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
                State = State == CompositeState.New ? CompositeState.New : CompositeState.Modified;
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
                State = State == CompositeState.New ? CompositeState.New : CompositeState.Modified;
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
                State = State == CompositeState.New ? CompositeState.New : CompositeState.Modified;
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
                State = State == CompositeState.New ? CompositeState.New : CompositeState.Modified;
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
                State = State == CompositeState.New ? CompositeState.New : CompositeState.Modified;
            }
        }


        [Command]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_RemoveHelp))]
        public void Remove()
        {
            Blogs.blogs.Remove(Id, true);
        }

        [Command]
        [Help(typeof(Resources), nameof(Resources.BlogComposite_SaveHelp))]
        public BlogComposite Save()
        {
            return this.Save<BlogComposite>();
        }
    }
}
