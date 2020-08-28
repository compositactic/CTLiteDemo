using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts.Attachments;
using CTLiteDemo.Presentation.Properties;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments
{
    [DataContract]
    [ParentProperty(nameof(AttachmentCompositeContainer.Post))]
    [CompositeContainer(nameof(AttachmentCompositeContainer.Attachments), nameof(Model.BlogApplications.Blogs.Posts.Post.Attachments), nameof(AttachmentCompositeContainer.attachments))]
    public class AttachmentCompositeContainer : Composite
    {
        public override CompositeState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PostComposite Post { get; private set; }

        internal AttachmentCompositeContainer(PostComposite postComposite)
        {
            this.InitializeCompositeContainer(out attachments, postComposite);
            _newAttachmentFunc = () => Post.PostModel.CreateNewAttachment();
        }

        private readonly Func<Attachment> _newAttachmentFunc;

        [NonSerialized]
        internal CompositeDictionary<long, AttachmentComposite> attachments;
        [DataMember]
        [Help(typeof(Resources), nameof(Resources.AttachmentCompositeContainer_AttachmentsHelp))]
        public ReadOnlyCompositeDictionary<long, AttachmentComposite> Attachments { get; private set; }

        [Command]
        [Help(typeof(Resources), nameof(Resources.AttachmentCompositeContainer_CreateNewAttachmentHelp))]
        [return: Help(typeof(Resources), nameof(Resources.AttachmentCompositeContainer_CreateNewAttachment_ReturnValueHelp))]
        public AttachmentComposite CreateNewAttachment(CompositeRootHttpContext context)
        {
            var newAttachment = new AttachmentComposite(_newAttachmentFunc.Invoke(), this)
            {
                State = CompositeState.New
            };

            var fileAttachment = context.Request.UploadedFiles.FirstOrDefault();

            var attachmentArchiveService = CompositeRoot.GetService<IAttachmentArchiveService>();
            attachmentArchiveService.ArchiveAttachment(fileAttachment, newAttachment);

            attachments.Add(newAttachment.Id, newAttachment);
            return newAttachment;
        }
    }
}
