using AppBroker.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppBroker.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("SignIn");
            
        }
        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SignIn(SignInForm forn)
        {
            return Ok();
        }
    }
}
