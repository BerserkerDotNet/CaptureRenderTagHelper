using Microsoft.AspNetCore.Mvc;
using CaptureRenderTagHelper.Sample.Models;

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
