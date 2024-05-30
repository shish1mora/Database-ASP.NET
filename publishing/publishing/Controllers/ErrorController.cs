using Microsoft.AspNetCore.Mvc;

namespace publishing.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index(string errorMessage)
        {
            ViewBag.errorMessage = errorMessage;
            return View();
        }
    }
}
