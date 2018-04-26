using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ScriptCaptureTagHelper.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class ScriptTagBenchmark
    {
        private ViewContext _viewContext;
        private ScriptRenderTagHelper renderTag;
        private TagHelperOutput renderOutput;
        private TagHelperContext renderContext;

        [Params(1_000, 10_000, 100_000)]
        public int StringLength { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _viewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext()
            };
            
            var random = new Random();
            var randomString = new string(Enumerable.Range(0, StringLength).Select(n => (char) random.Next()).ToArray());
            var captureOutput = CreateCaptureTagWith(randomString);
            this.renderOutput = CreateRenderTag();
            this.renderContext = CreateHelperContext();
            
            ProcessCaptureHelper(captureOutput, "current").GetAwaiter().GetResult();
            this.renderTag = new ScriptRenderTagHelper
            {
                Render = "current",
                ViewContext = _viewContext
            };
        }

        [Benchmark]
        public string Process()
        {
            renderTag.ProcessAsync(renderContext, renderOutput);
            return renderOutput.Content.GetContent();
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