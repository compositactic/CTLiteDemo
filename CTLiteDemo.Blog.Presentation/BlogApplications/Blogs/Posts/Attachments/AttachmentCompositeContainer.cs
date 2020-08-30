using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts.Attachments;
using CTLiteDemo.Presentation.Properties;
using System;
using System.Collections.Generic;
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
        [Help(typeof(Resources), nameof(Resources.AttachmentCompositeContainer_CreateNewAttachmentsHelp))]
        [return: Help(typeof(Resources), nameof(Resources.AttachmentCompositeContainer_CreateNewAttachment_ReturnValueHelp))]
        public AttachmentComposite[] CreateNewAttachments(CompositeRootHttpContext context, bool shouldArchiveAttachments)
        {
            var attachmentArchiveService = CompositeRoot.GetService<IAttachmentArchiveService>();
            var addedAttachments = new List<AttachmentComposite>();

            foreach(var uploadedFile in context.Request.UploadedFiles)
            {
                var newAttachment = new AttachmentComposite(_newAttachmentFunc.Invoke(), this) { State = CompositeState.New };

                if(shouldArchiveAttachments)
                    attachmentArchiveService.ArchiveAttachment(uploadedFile, newAttachment);

                attachments.Add(newAttachment.Id, newAttachment);
                addedAttachments.Add(newAttachment);
            }

            return addedAttachments.ToArray();
        }
    }
}
