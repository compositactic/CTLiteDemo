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
using CTLite.AspNetCore;
using CTLite.Data.MicrosoftSqlServer;
using CTLiteDemo.Presentation.BlogApplications;
using CTLiteDemo.Service.BlogApplications.Blogs.Posts.Attachments;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace CTLiteDemo.WebApi
{
    public class BlogApplicationController : CompositeRootControllerBase<BlogApplicationCompositeRoot>
    {
        public BlogApplicationController(IMemoryCache cache) : base(cache) { }
        protected override BlogApplicationCompositeRoot CreateCompositeRoot()
        {
            return new BlogApplicationCompositeRoot
            (
                MicrosoftSqlServerRepository.Create(),
                new AttachmentArchiveService()
            );
        }

        protected override void OnAfterExecute(IEnumerable<CompositeRootCommandResponse> commandResponses, CompositeRootHttpContext compositeRootHttpContext)
        {
            base.OnAfterExecute(commandResponses, compositeRootHttpContext);
        }

        protected override void OnBeforeExecute(IEnumerable<CompositeRootCommandRequest> commandRequests, CompositeRootHttpContext compositeRootHttpContext, IEnumerable<CompositeUploadedFile> uploadedFiles)
        {
            base.OnBeforeExecute(commandRequests, compositeRootHttpContext, uploadedFiles);
        }
    }
}
