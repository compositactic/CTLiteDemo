using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts.Attachments;
using CTLiteDemo.Presentation.Properties;
using System.IO;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments
{
    [DataContract]
    [KeyProperty(nameof(AttachmentComposite.Id), nameof(AttachmentComposite.OriginalId))]
    [ParentProperty(nameof(AttachmentComposite.Attachments))]
    [CompositeModel(nameof(AttachmentComposite.AttachmentModel))]
    public class AttachmentComposite : Composite
    {
        public override CompositeState State { get => AttachmentModel.State; set => AttachmentModel.State = value; }

        internal Attachment AttachmentModel;
        public AttachmentCompositeContainer Attachments { get; private set; }

        internal AttachmentComposite(Attachment attachment, AttachmentCompositeContainer attachmentCompositeContainer)
        {
            AttachmentModel = attachment;
            Attachments = attachmentCompositeContainer;
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.AttachmentComposite_FilePathHelp))]
        public string FilePath
        {
            get { return AttachmentModel.FilePath; }
            set
            {
                AttachmentModel.FilePath = value;
                NotifyPropertyChanged(nameof(AttachmentComposite.FilePath));
            }
        }

        [Command]
        public byte[] GetAttachment(CompositeRootHttpContext context)
        {
            var attachmentBytes = CompositeRoot.GetService<IAttachmentArchiveService>().GetAttachment(FilePath);
            context.Response.ContentType = ContentTypes.GetContentTypeFromFileExtension(Path.GetExtension(FilePath));
            return attachmentBytes;
        }

        [DataMember]
        [Help(typeof(Resources), nameof(Resources.AttachmentComposite_IdHelp))]
        public long Id
        {
            get { return AttachmentModel.Id; }
        }

        public long OriginalId { get { return AttachmentModel.OriginalId;  } }

        [Command]
        [Help(typeof(Resources), nameof(Resources.AttachmentComposite_RemoveHelp))]
        public void Remove()
        {
            Attachments.attachments.Remove(Id, true);
        }
    }
}
