using CTLite;
using CTLiteDemo.Presentation.BlogApplications.Blogs.Posts.Attachments;
using System.IO;

namespace CTLiteDemo.Service.BlogApplications.Blogs.Posts.Attachments
{
    public class AttachmentArchiveService : IAttachmentArchiveService
    {
        public AttachmentArchiveService() { }

        public CompositeRoot CompositeRoot { get; set; }

        public void ArchiveAttachment(CompositeUploadedFile compositeUploadedFile, AttachmentComposite attachment)
        {
            if (compositeUploadedFile != null)
            {
                var fileAttachmentPath = $"{Path.GetTempFileName()}{compositeUploadedFile.FileName}";
                File.WriteAllBytes(fileAttachmentPath, compositeUploadedFile.GetContent());
                attachment.FilePath = fileAttachmentPath;
            }
        }

        public byte[] GetAttachment(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
    }
}
