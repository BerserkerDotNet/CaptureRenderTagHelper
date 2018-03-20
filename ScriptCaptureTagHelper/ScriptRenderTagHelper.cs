using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ScriptCaptureTagHelper.Types;
using System.IO;
using System.Linq;
using System.Text;

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

            var capture = ViewContext.HttpContext.Items[key] as ScriptCapture;
            if (capture == null)
                return;

            output.TagName = null;
            var result = new StringBuilder();
            using (var tw = new StringWriter(result))
            {
                foreach (var block in capture.Blocks.OrderBy(b => b.Order))
                {
                    var tagBuilder = new TagBuilder("script");
                    tagBuilder.TagRenderMode = TagRenderMode.Normal;
                    tagBuilder.InnerHtml.AppendHtml(block.Content);
                    tagBuilder.MergeAttributes(block.Attributes, replaceExisting: true);
                    tagBuilder.WriteTo(tw, NullHtmlEncoder.Default);
                    tw.WriteLine();
                }
            }
            output.Content.SetHtmlContent(result.ToString());
        }
    }
}
