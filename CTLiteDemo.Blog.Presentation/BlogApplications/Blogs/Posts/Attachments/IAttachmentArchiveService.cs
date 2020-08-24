using CTLite;

namespace CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments
{
    public interface IAttachmentArchiveService : IService
    {
        void ArchiveAttachment(CompositeUploadedFile compositeUploadedFile, AttachmentComposite attachment);
    }
}
