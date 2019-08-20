using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ContentCaptureTagHelper.Types
{
    public struct ContentBlock
    {

        public ContentBlock(TagHelperContent content, Dictionary<string, object> attributes, string tag, bool noTag, int order, bool? canMerge)
        {
            Content = content;
            Attributes = attributes;
            Order = order;
            CanMerge = canMerge;
            Tag = tag;
            NoTag = noTag;
        }

        public TagHelperContent Content { get; }
        public int Order { get; }
        public bool? CanMerge { get; set; }
        public string Tag { get; set; }
        public bool NoTag { get; set; }
        public Dictionary<string, object> Attributes { get; }
    }
}
