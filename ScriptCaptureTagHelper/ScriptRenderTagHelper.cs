using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ScriptCaptureTagHelper
{
    /// <summary>
    /// Renders a script block that was stored by <see cref="ScriptCaptureTagHelper"/>
    /// </summary>
    [HtmlTargetElement("script", Attributes = "render")]
    public class ScriptRenderTagHelper : TagHelper
    {
        /// <summary>
        /// Unique id of the script block
        /// </summary>
        [HtmlAttributeName("render")]
        public string Render { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(Render))
                return;

            var key = $"Script_{Render}";
            if (!ViewContext.HttpContext.Items.ContainsKey(key))
                return;

            var content = ViewContext.HttpContext.Items[key].ToString();
            output.TagName = "script";
            output.Attributes.Clear();

            output.Content.SetHtmlContent(content);
        }
    }
}
