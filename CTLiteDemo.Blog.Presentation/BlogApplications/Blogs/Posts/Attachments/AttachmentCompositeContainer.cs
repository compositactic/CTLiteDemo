using CTLite;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments
{
    [DataContract]
    [ParentProperty(nameof(AttachmentCompositeContainer.Post))]
    [CompositeContainer(nameof(AttachmentCompositeContainer.Attachments), nameof(Model.BlogApplications.Blogs.Posts.Post.Attachments))]
    public class AttachmentCompositeContainer : Composite
    {
        public PostComposite Post { get; private set; }

        internal AttachmentCompositeContainer(PostComposite postComposite)
        {
            this.InitializeCompositeContainer(out attachments, postComposite);
        }

        [NonSerialized]
        internal CompositeDictionary<long, AttachmentComposite> attachments;
        [DataMember]
        public ReadOnlyCompositeDictionary<long, AttachmentComposite> Attachments { get; private set; }

        [Command]
        public AttachmentComposite CreateNewAttachment(CompositeRootHttpContext context)
        {
            var newAttachment = new AttachmentComposite(Post.PostModel.CreateNewAttachment(), this)
            {
                State = CompositeState.New
            };

            var fileAttachment = context.Request.UploadedFiles.FirstOrDefault();
            if (fileAttachment != null)
            {
                var fileAttachmentPath = Path.GetTempFileName();
                File.WriteAllBytes(fileAttachmentPath, fileAttachment.GetContent());
                newAttachment.FilePath = fileAttachmentPath;
            }

            attachments.Add(newAttachment.Id, newAttachment);
            return newAttachment;
        }
    }
}
