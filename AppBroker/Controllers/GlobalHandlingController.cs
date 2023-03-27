using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace AppBroker.Controllers
{
    public class GlobalHandlingController : Controller
    {
        public GlobalHandlingController()
        {
        }
        public IActionResult Index()
        {
            var handler = this.HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var message = handler?.Error.InnerException?.Message?? handler?.Error?.Message ?? "Internal Server Error";
            ViewBag.Message = message;
            return View();
        }
    }
}
