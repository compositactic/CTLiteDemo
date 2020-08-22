using CTLite;
using CTLiteDemo.Model.BlogApplications.Blogs.Posts.Attachments;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments
{
    [DataContract]
    [KeyProperty(nameof(AttachmentComposite.Id))]
    [ParentProperty(nameof(AttachmentComposite.Attachments))]
    [CompositeModel(nameof(AttachmentComposite.AttachmentModel))]
    public class AttachmentComposite : Composite
    {
        public Attachment AttachmentModel { get; }
        public AttachmentCompositeContainer Attachments { get; private set; }

        internal AttachmentComposite(Attachment attachment, AttachmentCompositeContainer attachmentCompositeContainer)
        {
            AttachmentModel = attachment;
            Attachments = attachmentCompositeContainer;
        }

        [DataMember]
        public string FilePath
        {
            get { return AttachmentModel.FilePath; }
            set
            {
                AttachmentModel.FilePath = value;
                NotifyPropertyChanged(nameof(AttachmentComposite.FilePath));
            }
        }

        [DataMember]
        public long Id
        {
            get { return AttachmentModel.Id; }
        }

        [Command]
        public void Remove()
        {
            Attachments.attachments.Remove(Id);
        }
    }
}
