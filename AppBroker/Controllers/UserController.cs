using AppBroker.Interfaces;
using AutoMapper;
using BusinessCore.Interfaces;
using Infrastructure;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppBroker.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService userService;
        private readonly IMapper Mapper;
        private readonly IHelperService helperService;
        public UserController(IUserService userService, IMapper Mapper, IHelperService helperService)
        {
            this.helperService = helperService;
            this.Mapper = Mapper;
            this.userService = userService;
        }
        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost("User/SignIn")]
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
        [HttpPost("User/Register")]
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

        public IActionResult UserManagement()
        {
            return View();
        }
        [HttpPost("User/GetGridUser")]
        [Produces("application/json")]
        public async Task<IActionResult> GetGridUser()
        {
            var form = HttpContext.Request.Form;
            var data = await userService.GridListUserQueryrable(form);
            var response = new GridDataTable<List<UserListDTO>>()
            {
                data = data,
                draw = Convert.ToInt16(form["draw"].FirstOrDefault() ?? "0"),
                recordsFiltered = data.Count(),
                recordsTotal = data.Count()
            };
            return new JsonResult(response);
        }
        public IActionResult AddNewUser()
        {
            return View();
        }
        [HttpPost("User/AddNewUser")]
        public async Task<IActionResult> AddNewUser(RegisterDTO form)
        {
            if (ModelState.IsValid)
            {
                var userid = "Admin";
                await userService.AddNewUser(form,userid);
                return Redirect("/User/UserManagement");
            }
            else
            {
                return View(form);
            }
        }
        public async Task<IActionResult> EditUser(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return View();
            var userdata = await userService.GetUserByUserId(id);
            var param = Mapper.Map<UserEditSubmitDTO>(userdata);
            return View(param);
        }
        [HttpPost("User/EditUser")]
        public async Task<IActionResult> EditUser(UserEditSubmitDTO form)
        {
            if (ModelState.IsValid)
            {
                var userid = "Admin";
                await userService.EditUser(form, userid);
                return Redirect("/User/UserManagement");
            }
            else
            {
                return View(form);
            }
        }
        [HttpPost("User/Delete")]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(string Id)
        {
            //return new JsonResult(true);
            if (string.IsNullOrWhiteSpace(Id)) return new JsonResult(false);
            await userService.DeleteUser(Id);
            return new JsonResult(true);
        }
        [HttpPost("User/Export")]
        public async Task<IActionResult> Export()
        {
            var data = await userService.ExportUserManagement();
            var exportData = Mapper.Map<List<UserExportDto>>(data);
            IExporter exporter = new ExcelExporter();
            var fileContent = await exporter.ExportAsByteArray(exportData);
            return File(fileContent, System.Net.Mime.MediaTypeNames.Application.Octet, "UserManegement.xlsx");
        }
        [HttpPost("User/Import")]
        public async Task<IActionResult> Import()
        {
            var files = HttpContext.Request.Form.Files;
            await this.helperService.UploadFile(files);
            return Redirect("/User/UserManagement");
        }
    }
}