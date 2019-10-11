using CaptureRenderTagHelper.Sample.Models;
using Microsoft.AspNetCore.Mvc;

namespace CaptureRenderTagHelper.Sample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new DisplayModel());
        }
    }
}
