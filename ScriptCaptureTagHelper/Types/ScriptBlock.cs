using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ScriptCaptureTagHelper.Types
{
    public struct ScriptBlock
    {
        public ScriptBlock(TagHelperContent content, Dictionary<string, object> attributes, int order)
        {
            Content = content;
            Attributes = attributes;
            Order = order;
        }

        public TagHelperContent Content { get; }
        public int Order { get; }
        public Dictionary<string, object> Attributes { get; }
    }
}
