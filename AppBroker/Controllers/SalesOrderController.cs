using Microsoft.AspNetCore.Mvc;

namespace AppBroker.Controllers
{
    public class SalesOrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
