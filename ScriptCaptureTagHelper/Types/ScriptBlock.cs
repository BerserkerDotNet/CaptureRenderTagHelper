using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ScriptCaptureTagHelper.Types
{
    public struct ScriptBlock
    {
        public ScriptBlock(TagHelperContent content, Dictionary<string, object> attributes, int order, bool? canMerge)
        {
            Content = content;
            Attributes = attributes;
            Order = order;
            CanMerge = canMerge;
        }

        public TagHelperContent Content { get; }
        public int Order { get; }
        public bool? CanMerge { get; set; }
        public Dictionary<string, object> Attributes { get; }
    }
}
