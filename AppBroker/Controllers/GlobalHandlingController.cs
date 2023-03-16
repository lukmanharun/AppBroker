using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace AppBroker.Controllers
{
    public class GlobalHandlingController : Controller
    {
        public IActionResult Index()
        {
            var handler = this.HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var message = handler?.Error?.Message ?? "Internal server error";
            Serilog.Log.Logger.Error(message);
            ViewBag.Message = message;
            return View();
        }
    }
}
