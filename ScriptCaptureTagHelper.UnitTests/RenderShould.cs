using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptCaptureTagHelper.UnitTests
{
    public class RenderShould
    {
        private ViewContext _viewContext;

        [SetUp]
        public void Setup()
        {
            _viewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }
        
        [Test]
        public async Task HaveEmptyCaptureOutput()
        {
            var content = await LayCaptureTag("console.log('Foo');");
            
            content.Should().BeEmpty();
        }

        [TestCase("console.log('Foo');")]
        [TestCase("console.log('Bar');")]
        public async Task RenderCaptureOutput(string captureContent)
        {
            await LayCaptureTag(captureContent);

            var content = await LayRenderTag();

            content.Should().Be($"<script>{captureContent}</script>{Environment.NewLine}");
        }

        [TestCase("1", "2")]
        [TestCase("2", "1")]
        public async Task RespectPriority(string script1, string script2)
        {
            await LayCaptureTag(script1);
            await LayCaptureTag(script2);

            var content = await LayRenderTag();

            content.Should().Be($"<script>{script1}</script>{Environment.NewLine}" +
                                $"<script>{script2}</script>{Environment.NewLine}");
        }
        
        [Test]
        public async Task CaptureScriptTagAttributes()
        {
            await LayCaptureTag("", ("src", "some good CDN"), ("integrity", "sha256-bla"));
            await LayCaptureTag("console.log('Foo 2');");
            
            var content = await LayRenderTag();

            content.Should().Be($"<script integrity=\"sha256-bla\" src=\"some good CDN\"></script>{Environment.NewLine}" +
                                $"<script>console.log('Foo 2');</script>{Environment.NewLine}");
        }
        
        private Task<string> LayCaptureTag(string content) 
            => LayCaptureTag(content, "UniqueValue", int.MaxValue);
        
        private Task<string> LayCaptureTag(string content, params (string name, string value)[] attrs) 
            => LayCaptureTag(content, "UniqueValue", int.MaxValue, attrs);
        
        private async Task<string> LayCaptureTag(string content, string captureId = "UniqueValue", int? priority = null, params (string name, string value)[] attrs)
        {
            var defaultTags = new[] {new TagHelperAttribute("capture", captureId)}; 
            var allAttrs = new TagHelperAttributeList(defaultTags.Concat(attrs.Select(a => new TagHelperAttribute(a.name, a.value))));
            var output = new TagHelperOutput("script", allAttrs, (r, e) => Task.FromResult(new DefaultTagHelperContent().SetHtmlContent(content)));
            
            var captureTag = new ScriptCaptureTagHelper
            {
                Capture = captureId,
                Priority = priority,
                ViewContext = _viewContext
            };
            
            await captureTag.ProcessAsync(CreateHelperContext(allAttrs), output);

            return output.Content.GetContent();
        }

        private async Task<string> LayRenderTag(string renderId = "UniqueValue")
        {
            var allAttrs = new TagHelperAttributeList(new[] {new TagHelperAttribute("render", renderId)});
            var output = new TagHelperOutput("script",
                allAttrs,
                (result, encoder) => Task.FromResult(new DefaultTagHelperContent().SetHtmlContent(string.Empty)));
            
            var renderTag = new ScriptRenderTagHelper
            {
                Render = renderId,
                ViewContext = _viewContext
            };
            
            await renderTag.ProcessAsync(CreateHelperContext(), output);
            
            return output.Content.GetContent();
        }
        
        private static TagHelperContext CreateHelperContext(TagHelperAttributeList attrs = null)
            => new TagHelperContext(
                attrs ?? new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString("N"));
    }
}