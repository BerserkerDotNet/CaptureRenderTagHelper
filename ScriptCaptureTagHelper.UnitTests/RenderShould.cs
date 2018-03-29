using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace ScriptCaptureTagHelper.UnitTests
{
    public class RenderShould
    {
        private readonly ViewContext _viewContext;

        public RenderShould()
        {
            _viewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }
        
        [Fact]
        public async Task HaveEmptyCaptureOutput()
        {
            var captureOutput = CreateCaptureTagWith("console.log('Foo');");
            
            await ProcessCaptureHelper(captureOutput);

            var content = captureOutput.Content.GetContent();
            content.Should().Be("");
        }

        [Theory]
        [InlineData("console.log('Foo');")]
        [InlineData("console.log('Bar');")]
        public async Task RenderCaptureOutput(string captureContent)
        {
            var captureOutput = CreateCaptureTagWith(captureContent);
            var renderOutput = CreateRenderTag();
            await ProcessCaptureHelper(captureOutput);
            
            await ProcessRenderHelper(renderOutput);
            
            var content = renderOutput.Content.GetContent();
            content.Should().Be($"{captureContent}{Environment.NewLine}");
        }

        [Theory]
        [InlineData("1", "2")]
        [InlineData("2", "1")]
        public async Task RespectPriority(string script1, string script2)
        {
            var captureOutput1 = CreateCaptureTagWith(script1);
            var captureOutput2 = CreateCaptureTagWith(script2);
            var renderOutput = CreateRenderTag();
            await ProcessCaptureHelper(captureOutput1);
            await ProcessCaptureHelper(captureOutput2);
            
            await ProcessRenderHelper(renderOutput);
            
            var content = renderOutput.Content.GetContent();
            content.Should().Be($"{script1}{Environment.NewLine}{script2}{Environment.NewLine}");
        }

        private async Task ProcessRenderHelper(TagHelperOutput tagHelperOutput, string name = "UniqueValue")
        {
            var renderTag = new ScriptRenderTagHelper
            {
                Render = name,
                ViewContext = _viewContext
            };
            
            await renderTag.ProcessAsync(CreateHelperContext(), tagHelperOutput);
        }
        
        private async Task ProcessCaptureHelper(TagHelperOutput output, string name = "UniqueValue", int? priority = null)
        {
            var captureTag = new ScriptCaptureTagHelper
            {
                Capture = name,
                Priority = priority,
                ViewContext = _viewContext
            };
            
            await captureTag.ProcessAsync(CreateHelperContext(), output);                   
        }

        private static TagHelperOutput CreateCaptureTagWith(string content)
            => new TagHelperOutput("capture",
                new TagHelperAttributeList(),
                (result, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetHtmlContent(content);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
        
        private static TagHelperOutput CreateRenderTag()
            => new TagHelperOutput("render",
                new TagHelperAttributeList(),
                (result, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetHtmlContent(string.Empty);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

        private static TagHelperContext CreateHelperContext()
            => new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString("N"));
    }
}