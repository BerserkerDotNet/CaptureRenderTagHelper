using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
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

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(Capture))
                return;

            var content = await output.GetChildContentAsync();
            var key = $"Script_{Capture}";
            ViewContext.HttpContext.Items.Add(key, content.GetContent());
            output.SuppressOutput();
        }
    }
}
