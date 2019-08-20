using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ContentCaptureTagHelper.Types;
using System.Linq;
using System.Threading.Tasks;

namespace ContentCaptureTagHelper
{
    /// <summary>
    /// Captures a script block for future rendering and suppresses output
    /// </summary>
    [HtmlTargetElement(Attributes = "capture")]
    public class ContentCaptureTagHelper : TagHelper
    {
        const string CaptureAttributeName = "capture";
        const string PriorityAttributeName = "priority";
        const string AllowMergeAttributeName = "allow-merge";
        const string NoTagAttributeName = "nooutputtag";

        private static readonly string[] SystemAttributes = new[] { CaptureAttributeName, PriorityAttributeName, AllowMergeAttributeName };

        /// <summary>
        /// Unique id of the script block
        /// </summary>
        [HtmlAttributeName(CaptureAttributeName)]
        public string Capture { get; set; }

        /// <summary>
        /// Defines an order in which a captured block should be rendered
        /// </summary>
        [HtmlAttributeName(PriorityAttributeName)]
        public int? Priority { get; set; }

        /// <summary>
        /// Get or sets whether the captured block can be merged with others or no.
        /// </summary>
        [HtmlAttributeName(AllowMergeAttributeName)]
        public bool? AllowMerge { get; set; }
        
        /// <summary>
        /// Get or sets whether the captured block will be output with or without an enclosing tag.
        /// </summary>
        [HtmlAttributeName(NoTagAttributeName)]
        public bool? NoTag { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(Capture))
                return;
            
            var attributes = context.AllAttributes
                .Where(a => !SystemAttributes.Contains(a.Name))
                .ToDictionary(k => k.Name, v => v.Value);
            var content = await output.GetChildContentAsync();
            var key = $"Script_{Capture}";
            ContentCapture capture = null;
            if (ViewContext.HttpContext.Items.ContainsKey(key))
            {
                capture = ViewContext.HttpContext.Items[key] as ContentCapture;
            }
            
            if (capture == null)
            {
                capture = new ContentCapture();
                ViewContext.HttpContext.Items.Add(key, capture);
            }
            
            var order = Priority ?? int.MaxValue;
            capture.Add(content, attributes, output.TagName, NoTag.GetValueOrDefault(false), order, AllowMerge);
            output.SuppressOutput();
        }
    }
}
