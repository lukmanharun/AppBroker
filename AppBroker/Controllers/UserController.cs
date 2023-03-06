using AppBroker.BusinessCore.Entity.DTO;
using BusinessCore.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppBroker.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService userService;
        public UserController(IUserService userService)
        {
            this.userService = userService;
        }
        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> SignInAsync(SignInDTO form)
        {
            if (ModelState.IsValid)
            {
                var token = await userService.LoginAsync(form);
                var claims = new Claim[]
                {
                    new Claim("AccessToken",token)
                };
                var claimIdentity = new ClaimsIdentity(claims);
                this.HttpContext.User.AddIdentity(claimIdentity);
                return Redirect("/User/UserManagement");
            }
            else
            {
                return View(form);
            }
        }
        public IActionResult Register() 
        { 
            return View();
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RegisterDTO form)
        {
            if (ModelState.IsValid) 
            {
                await userService.RegisterAsync(form);
                return Redirect("/User/SignIn");
            }
            else
            {
                return View(form);
            }
        }

        public async Task<IActionResult> UserManagement()
        {
            var data = await userService.ListUser();
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> UserManagement(UserSubmitDTO formSubmit)
        {
            return View(formSubmit);
        }

        public IActionResult Logout()
        {
            return View();
        }
    }
}
