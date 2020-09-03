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
