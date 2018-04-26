using Microsoft.AspNetCore.Mvc;
using ScriptCaptureTagHelper.Sample.Models;

namespace ScriptCaptureTagHelper.Sample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {    
            return View(new DisplayModel());
        }
    }
}
