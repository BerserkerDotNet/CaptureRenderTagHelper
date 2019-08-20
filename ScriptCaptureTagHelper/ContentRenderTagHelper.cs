using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ContentCaptureTagHelper.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ContentCaptureTagHelper
{
    /// <summary>
    /// Renders a content block that was stored by <see cref="ContentCaptureTagHelper"/>
    /// </summary>
    [HtmlTargetElement(Attributes = "render")]
    public class ContentRenderTagHelper : TagHelper
    {
        private const string DefaultTag = "script";

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

        /// <summary>
        /// Get or sets whether the renderer should do duplicate detection on src attribute
        /// </summary>
        [HtmlAttributeName("no-duplicate-source")]
        public bool NoDuplicateSource { get; set; } = true;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(Render))
                return;

            var key = $"Script_{Render}";
            if (!ViewContext.HttpContext.Items.ContainsKey(key) ||
                !(ViewContext.HttpContext.Items[key] is ContentCapture capture))
                return;

            output.TagName = null;
            output.Content.SetHtmlContent(new HelperResult(async tw => await RenderBlocks(tw, capture)));
        }

        private async Task RenderBlocks(TextWriter tw, ContentCapture capture)
        {
            const string srcAttribute = "src";
            var blocks = capture.Blocks;

            if (NoDuplicateSource)
            {
                blocks = blocks
                    .GroupBy(b => b.Attributes.ContainsKey(srcAttribute) ? b.Attributes[srcAttribute] : Guid.NewGuid())
                    .Select(b => b.First());
            }

            var orderedBlocks = blocks.OrderBy(b => b.Order);
            var mergableBlocks = orderedBlocks.Where(b =>
                ((AutoMerge && (!b.CanMerge.HasValue || b.CanMerge.Value)) ||
                (!AutoMerge && b.CanMerge.HasValue && b.CanMerge.Value))
                && !b.Content.IsEmptyOrWhiteSpace);
            var otherBlocks = orderedBlocks.Except(mergableBlocks);

            await RenderMergedBlocks(tw, mergableBlocks);
            await RenderSeparateBlocks(tw, otherBlocks);
        }

        private async Task RenderSeparateBlocks(TextWriter tw, IEnumerable<ContentBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (!block.NoTag)
                {
                    var tagBuilder = new TagBuilder(block.Tag ?? DefaultTag)
                    {
                        TagRenderMode = TagRenderMode.Normal
                    };
                    tagBuilder.InnerHtml.AppendHtml(block.Content);
                    tagBuilder.MergeAttributes(block.Attributes, replaceExisting: true);
                    tagBuilder.WriteTo(tw, NullHtmlEncoder.Default);
                } else
                {
                    block.Content.WriteTo(tw, NullHtmlEncoder.Default);
                }

                await tw.WriteLineAsync();
            }
        }

        private async Task RenderMergedBlocks(TextWriter tw, IEnumerable<ContentBlock> blocks)
        {
            if (!blocks.Any())
                return;

            var tagBuilder = new TagBuilder(blocks.First().Tag ?? DefaultTag)
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
