using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ScriptCaptureTagHelper.Types;
using System.Linq;

namespace ScriptCaptureTagHelper
{
    /// <summary>
    /// Renders a script block that was stored by <see cref="ScriptCaptureTagHelper"/>
    /// </summary>
    [HtmlTargetElement("script", Attributes = "render")]
    public class ScriptRenderTagHelper : TagHelper
    {
        private const string ScriptTag = "script";

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
            if (!ViewContext.HttpContext.Items.ContainsKey(key) ||
                !(ViewContext.HttpContext.Items[key] is ScriptCapture capture))
                return;

            output.Content.SetHtmlContent(new HelperResult(async tw =>
            {
                foreach (var block in capture.Blocks.OrderBy(b => b.Order))
                {
                    var tagBuilder = new TagBuilder(ScriptTag)
                    {
                        TagRenderMode = TagRenderMode.Normal
                    };
                    tagBuilder.InnerHtml.AppendHtml(block.Content);
                    tagBuilder.MergeAttributes(block.Attributes, replaceExisting: true);
                    tagBuilder.WriteTo(tw, NullHtmlEncoder.Default);
                    
                    await tw.WriteLineAsync();
                }
            }));
        }
    }
}
