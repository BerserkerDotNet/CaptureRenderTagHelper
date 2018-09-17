using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;
using ScriptCaptureTagHelper.Types;
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
            var content = await DoCapture("console.log('Foo');");
            
            content.Should().BeEmpty();
        }

        [Test]
        public async Task NotRenderInitialScriptTag()
        {
            await DoCapture("console.log('Foo');");
            var renderTag = await DoRenderTag();

            renderTag.TagName.Should().BeNull();
        }

        [Test]
        public async Task StoreSameKeyCapturesInOneBucket()
        {
            const string Bucket1Key = "Bucket 1";
            const string Bucket2Key = "Bucket 2";

            await DoCapture("Bucket 1 script 1", captureId: Bucket1Key);
            await DoCapture("Bucket 1 script 2", captureId: Bucket1Key);
            await DoCapture("Bucket 2 script 1", captureId: Bucket2Key);

            _viewContext.HttpContext.Items.Keys.Should().HaveCount(2);

            var bucket1 = _viewContext.HttpContext.Items[$"Script_{Bucket1Key}"] as ScriptCapture;
            var bucket2 = _viewContext.HttpContext.Items[$"Script_{Bucket2Key}"] as ScriptCapture;

            bucket1.Blocks.Should().HaveCount(2);
            bucket2.Blocks.Should().HaveCount(1);
        }

        [Test]
        public async Task AutoMergeOnRenderIfRequested()
        {
            const string script1 = "console.log('Foo')";
            const string script2 = "console.log('Bar')";

            await DoCapture(script1);
            await DoCapture(script2);

            var content = await DoRender(autoMerge: true);

            content.Should().Be("<script>" +
                script1 +
                script2 +
                "</script>" +
                Environment.NewLine);
        }

        [Test]
        public async Task AutoMergeOnlyScriptsWithContent()
        {
            const string script1 = "console.log('Foo');";
            const string script2 = "console.log('Bar');";

            await DoCapture(script1);
            await DoCapture(script2);
            await DoCapture(string.Empty, attrs: ("src", "https://goodcdn"));

            var content = await DoRender(autoMerge: true);

            content.Should().Be("<script>" +
                script1 +
                script2 +
                "</script>" + Environment.NewLine +
                "<script src=\"https://goodcdn\"></script>" + Environment.NewLine);
        }

        [Test]
        public async Task AutoMergeRespectPriority()
        {
            const string script1 = "console.log('Foo')";
            const string script2 = "console.log('Bar')";

            await DoCapture(script1, priority: 2);
            await DoCapture(script2, priority: 1);

            var content = await DoRender(autoMerge: true);

            content.Should().Be("<script>" +
                script2 +
                script1 +
                "</script>" +
                Environment.NewLine);
        }

        [Test]
        public async Task NotAutoMergeScriptsThatMarkedAsNotMerge()
        {
            const string script1 = "console.log('Foo')";
            const string script2 = "console.log('Bar')";
            const string script3 = "console.log('Buz')";

            await DoCapture(script1);
            await DoCapture(script2, allowMerge: false);
            await DoCapture(script3);

            var content = await DoRender(autoMerge: true);

            content.Should().Be("<script>" +
                script1 +
                script3 +
                "</script>" +
                Environment.NewLine +
                $"<script>{script2}</script>" +
                Environment.NewLine);
        }

        [Test]
        public async Task NotMergeScriptsReferencesEvenIfMarkedAsMergable()
        {
            const string script1 = "console.log('Foo')";

            await DoCapture(script1);
            await DoCapture(string.Empty, allowMerge: true, attrs: ("src", "https://goodcdn"));
            var content = await DoRender(autoMerge: true);

            content.Should().Be("<script>" +
                script1 +
                "</script>" +
                Environment.NewLine +
                "<script src=\"https://goodcdn\"></script>" +
                Environment.NewLine);
        }

        [Test]
        public async Task MergeBlocksMarkedAsMergable()
        {
            const string script1 = "console.log('Foo')";
            const string script2 = "console.log('Bar')";

            await DoCapture(script1, true);
            await DoCapture(script2, true);

            var content = await DoRender(autoMerge: false);

            content.Should().Be("<script>" +
                script1 +
                script2 +
                "</script>" +
                Environment.NewLine);
        }

        [TestCase("console.log('Foo');")]
        [TestCase("console.log('Bar');")]
        public async Task RenderCaptureOutput(string captureContent)
        {
            await DoCapture(captureContent);

            var content = await DoRender();

            content.Should().Be($"<script>{captureContent}</script>{Environment.NewLine}");
        }

        [TestCase("console.log('First')", "console.log('Second')")]
        [TestCase("console.log('Second')", "console.log('First')")]
        public async Task RespectPriority(string script1, string script2)
        {
            await DoCapture(script1, priority: 2);
            await DoCapture(script2, priority: 1);

            var content = await DoRender();

            content.Should().Be($"<script>{script2}</script>{Environment.NewLine}" +
                                $"<script>{script1}</script>{Environment.NewLine}");
        }
        
        [Test]
        public async Task CaptureScriptTagAttributes()
        {
            await DoCapture("", ("src", "some good CDN"), ("integrity", "sha256-bla"));
            await DoCapture("console.log('Foo 2');");
            
            var content = await DoRender();

            content.Should().Be($"<script integrity=\"sha256-bla\" src=\"some good CDN\"></script>{Environment.NewLine}" +
                                $"<script>console.log('Foo 2');</script>{Environment.NewLine}");
        }

        [Test]
        public async Task DoNotRenderDuplicate()
        {
            var script = "console.log('Foo')";
            await DoCapture(string.Empty, attrs: ("src", "https://goodcdn/foo.js"));
            await DoCapture(script);
            await DoCapture(string.Empty, attrs: ("src", "https://goodcdn/foo.js"));

            var content = await DoRender();

            content.Should().Be("<script src=\"https://goodcdn/foo.js\"></script>" +
                Environment.NewLine +
                "<script>" +
                script +
                "</script>" +
                Environment.NewLine);
        }

        [Test]
        public async Task RenderDuplicateIfDuplicateDetectionDisabled()
        {
            var script = "console.log('Foo')";
            await DoCapture(string.Empty, attrs: ("src", "https://goodcdn/foo.js"));
            await DoCapture(script);
            await DoCapture(string.Empty, attrs: ("src", "https://goodcdn/foo.js"));

            var content = await DoRender(noDuplicateSource: false);

            content.Should().Be("<script src=\"https://goodcdn/foo.js\"></script>" +
                Environment.NewLine +
                "<script>" +
                script +
                "</script>" +
                Environment.NewLine +
                "<script src=\"https://goodcdn/foo.js\"></script>" +
                Environment.NewLine);
        }

        private Task<string> DoCapture(string content)
            => DoCapture(content, "UniqueValue", int.MaxValue, null);

        private Task<string> DoCapture(string content, bool allowMerge)
            => DoCapture(content, "UniqueValue", int.MaxValue, allowMerge);

        private Task<string> DoCapture(string content, params (string name, string value)[] attrs)
            => DoCapture(content, "UniqueValue", int.MaxValue, null, attrs);

        private Task<string> DoCapture(string content, bool? allowMerge = null, params (string name, string value)[] attrs) 
            => DoCapture(content, "UniqueValue", int.MaxValue, allowMerge, attrs);
        
        private async Task<string> DoCapture(string content, string captureId = "UniqueValue", int? priority = null, bool? allowMerge = null, params (string name, string value)[] attrs)
        {
            var defaultTags = new[] { new TagHelperAttribute("capture", captureId) };
            var allAttrs = new TagHelperAttributeList(defaultTags.Concat(attrs.Select(a => new TagHelperAttribute(a.name, a.value))));
            var output = new TagHelperOutput("script", allAttrs, (r, e) => Task.FromResult(new DefaultTagHelperContent().SetHtmlContent(content)));
            
            var captureTag = new ScriptCaptureTagHelper
            {
                Capture = captureId,
                Priority = priority,
                AllowMerge = allowMerge,
                ViewContext = _viewContext
            };
            
            await captureTag.ProcessAsync(CreateHelperContext(allAttrs), output);

            return output.Content.GetContent();
        }

        private async Task<string> DoRender(string renderId = "UniqueValue", bool autoMerge = false, bool noDuplicateSource = true)
        {
            var tag = await DoRenderTag(renderId, autoMerge, noDuplicateSource);
            return tag.Content.GetContent();
        }

        private async Task<TagHelperOutput> DoRenderTag(string renderId = "UniqueValue", bool autoMerge = false, bool noDuplicateSource = true)
        {
            var allAttrs = new TagHelperAttributeList(new[] { new TagHelperAttribute("render", renderId) });
            var output = new TagHelperOutput("script",
                allAttrs,
                (result, encoder) => Task.FromResult(new DefaultTagHelperContent().SetHtmlContent(string.Empty)));

            var renderTag = new ScriptRenderTagHelper
            {
                Render = renderId,
                AutoMerge = autoMerge,
                NoDuplicateSource = noDuplicateSource,
                ViewContext = _viewContext
            };

            await renderTag.ProcessAsync(CreateHelperContext(), output);

            return output;
        }

        private static TagHelperContext CreateHelperContext(TagHelperAttributeList attrs = null)
            => new TagHelperContext(
                attrs ?? new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString("N"));
    }
}