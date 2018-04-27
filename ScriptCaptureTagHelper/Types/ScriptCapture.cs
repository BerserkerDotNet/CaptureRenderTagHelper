using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ScriptCaptureTagHelper.Types
{
    public class ScriptCapture
    {
        private readonly List<ScriptBlock> _scriptBlocks = new List<ScriptBlock>();

        public void Add(TagHelperContent content, Dictionary<string, object> attributes, int order, bool? canMerge = null)
        {
            var block = new ScriptBlock(content, attributes, order, canMerge);
            lock (_scriptBlocks)
            {
                _scriptBlocks.Add(block);
            }
        }

        public IEnumerable<ScriptBlock> Blocks => _scriptBlocks;
    }
}
