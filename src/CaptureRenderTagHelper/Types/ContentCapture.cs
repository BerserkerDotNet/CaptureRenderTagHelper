using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CaptureRenderTagHelper.Types
{
    public class ContentCapture
    {
        private readonly List<ContentBlock> _contentBlocks = new List<ContentBlock>();

        public IEnumerable<ContentBlock> Blocks => _contentBlocks;

        public void Add(TagHelperContent content, Dictionary<string, object> attributes, string tag, bool noTag, int order, bool? canMerge = null)
        {
            var block = new ContentBlock(content, attributes, tag, noTag, order, canMerge);
            lock (_contentBlocks)
            {
                _contentBlocks.Add(block);
            }
        }
    }
}
