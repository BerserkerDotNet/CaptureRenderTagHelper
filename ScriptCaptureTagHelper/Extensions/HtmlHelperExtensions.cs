using ContentCaptureTagHelper.Types;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContentCaptureTagHelper.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static bool HasCaptured(this IHtmlHelper htmlHelper, string name)
        {            
            var key = $"Script_{name}";
            if (htmlHelper.ViewContext.HttpContext.Items.ContainsKey(key) &&
                (htmlHelper.ViewContext.HttpContext.Items[key] is ContentCapture))
                return true;

            return false;
        }

        public static string IfCaptured(this IHtmlHelper htmlHelper, string name, string output)
        {
            if (HasCaptured(htmlHelper, name))
                return output;

            return string.Empty;
        }
    }
}
