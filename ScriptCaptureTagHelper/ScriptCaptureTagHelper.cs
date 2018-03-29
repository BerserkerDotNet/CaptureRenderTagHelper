using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ScriptCaptureTagHelper.Types;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptCaptureTagHelper
{
    /// <summary>
    /// Captures a script block for future rendering and suppresses output
    /// </summary>
    [HtmlTargetElement("script", Attributes = "capture")]
    public class ScriptCaptureTagHelper : TagHelper
    {
        /// <summary>
        /// Unique id of the script block
        /// </summary>
        [HtmlAttributeName("capture")]
        public string Capture { get; set; }

        /// <summary>
        /// Defines an order in which a captured block should be rendered
        /// </summary>
        [HtmlAttributeName("priority")]
        public int? Priority { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(Capture))
                return;
            
            var attributes = context.AllAttributes
                .Where(a => !string.Equals(a.Name, "capture", System.StringComparison.OrdinalIgnoreCase) && 
                            !string.Equals(a.Name, "priority", System.StringComparison.OrdinalIgnoreCase))
                .ToDictionary(k => k.Name, v => v.Value);
            var content = await output.GetChildContentAsync();
            var key = $"Script_{Capture}";
            ScriptCapture capture = null;
            if (ViewContext.HttpContext.Items.ContainsKey(key))
            {
                capture = ViewContext.HttpContext.Items[key] as ScriptCapture;
            }
            
            if (capture == null)
            {
                capture = new ScriptCapture();
                ViewContext.HttpContext.Items.Add(key, capture);
            }
            
            var order = Priority ?? int.MaxValue;
            capture.Add(content, attributes, order);
            output.SuppressOutput();
        }
    }
}
