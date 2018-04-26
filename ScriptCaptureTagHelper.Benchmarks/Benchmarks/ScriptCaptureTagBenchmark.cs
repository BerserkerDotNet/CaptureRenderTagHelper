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
    public class ScriptCaptureTagBenchmark
    {
        private ViewContext _viewContext;
        private ScriptCaptureTagHelper _captureTag;
        private TagHelperOutput _captureOutput;
        private TagHelperContext _captureContext;

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
            var randomString = new string(Enumerable.Range(0, StringLength).Select(n => (char)random.Next()).ToArray());
            _captureOutput = CreateCaptureTagWith(randomString);
            _captureContext = CreateHelperContext();
            _captureTag = new ScriptCaptureTagHelper
            {
                Capture = "UniqueValue",
                Priority = null,
                ViewContext = _viewContext
            };
        }

        [Benchmark]
        public async Task<string> Capture()
        {
            await _captureTag.ProcessAsync(_captureContext, _captureOutput);
            return _captureOutput.Content.GetContent();
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

        private static TagHelperContext CreateHelperContext()
            => new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString("N"));
    }
}