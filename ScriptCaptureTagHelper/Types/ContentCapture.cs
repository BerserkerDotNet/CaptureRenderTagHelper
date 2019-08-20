using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ContentCaptureTagHelper.Types
{
    public class ContentCapture
    {
        private readonly List<ContentBlock> _scriptBlocks = new List<ContentBlock>();

        public void Add(TagHelperContent content, Dictionary<string, object> attributes, string tag, bool noTag, int order, bool? canMerge = null)
        {
            var block = new ContentBlock(content, attributes, tag, noTag, order, canMerge);
            lock (_scriptBlocks)
            {
                _scriptBlocks.Add(block);
            }
        }

        public IEnumerable<ContentBlock> Blocks => _scriptBlocks;
    }
}
