using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ScriptCaptureTagHelper.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// Get or sets whether the renderer should attempt to merge captured blocks into one.
        /// </summary>
        [HtmlAttributeName("auto-merge")]
        public bool AutoMerge { get; set; }

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

            output.TagName = null;
            output.Content.SetHtmlContent(new HelperResult(async tw => await RenderBlocks(tw, capture)));
        }

        private async Task RenderBlocks(TextWriter tw, ScriptCapture capture)
        {
            var orderedBlocks = capture.Blocks.OrderBy(b => b.Order);
            var mergableBlocks = orderedBlocks.Where(b => 
                ((AutoMerge && (!b.CanMerge.HasValue || b.CanMerge.Value)) ||
                (!AutoMerge && b.CanMerge.HasValue && b.CanMerge.Value))
                && !b.Content.IsEmptyOrWhiteSpace);
            var otherBlocks = orderedBlocks.Except(mergableBlocks);

            await RenderMergedBlocks(tw, mergableBlocks);
            await RenderSeparateBlocks(tw, otherBlocks);
        }

        private async Task RenderSeparateBlocks(TextWriter tw, IEnumerable<ScriptBlock> blocks)
        {
            foreach (var block in blocks)
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
        }

        private async Task RenderMergedBlocks(TextWriter tw, IEnumerable<ScriptBlock> blocks)
        {
            if (!blocks.Any())
                return;

            var tagBuilder = new TagBuilder(ScriptTag)
            {
                TagRenderMode = TagRenderMode.Normal
            };

            foreach (var block in blocks)
            {
                tagBuilder.InnerHtml.AppendHtml(block.Content);
                tagBuilder.MergeAttributes(block.Attributes, replaceExisting: true);
            }

            tagBuilder.WriteTo(tw, NullHtmlEncoder.Default);
            await tw.WriteLineAsync();
        }
    }
}
