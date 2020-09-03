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
