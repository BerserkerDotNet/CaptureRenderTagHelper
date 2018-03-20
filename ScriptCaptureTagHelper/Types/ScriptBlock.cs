using System.Collections.Generic;

namespace ScriptCaptureTagHelper.Types
{
    public class ScriptBlock
    {
        public ScriptBlock(string content, Dictionary<string, object> attributes, int order)
        {
            Content = content;
            Attributes = attributes;
            Order = order;
        }

        public string Content { get; }
        public int Order { get; }
        public Dictionary<string, object> Attributes { get; }
    }
}
