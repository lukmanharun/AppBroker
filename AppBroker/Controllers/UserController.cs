﻿using AppBroker.Interfaces;
using AutoMapper;
using Infrastructure;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Magicodes.ExporterAndImporter.Core.Models;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using BusinessCore;

namespace AppBroker.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService userService;
        private readonly IMapper Mapper;
        private readonly IHelperService helperService;
        private readonly ILogger<UserController> logger;
        //private readonly IRabbitMQService rabbitMQService;
        private readonly AppDbContext dbContext;
        public UserController(IUserService userService, IMapper Mapper, IHelperService helperService
            ,ILogger<UserController> logger//, IRabbitMQService rabbitMQService
            , AppDbContext dbContext
            )
        {
            //this.rabbitMQService = rabbitMQService;
            this.helperService = helperService;
            this.Mapper = Mapper;
            this.userService = userService;
            this.logger = logger;
            this.dbContext = dbContext;

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
            var result = new JsonResult(data);
            return result;
        }
        [AllowAnonymous]
        public IActionResult SignIn(string? returnUrl)
        {
            return View();
        }
        [HttpPost("User/SignIn")]
        [AllowAnonymous]
        public async Task<IActionResult> SignInAsync(SignInDTO form)
        {
            if (ModelState.IsValid)
            {
                var r = await userService.Authentication(form);
                if(!r.success)
                {
                    if(r.errors.Keys.Contains("Email"))
                        ModelState.AddModelError<SignInDTO>(s => s.Email, r.errors.Values.FirstOrDefault());
                    else if (r.errors.Keys.Contains("Password"))
                        ModelState.AddModelError<SignInDTO>(s => s.Password, r.errors.Values.FirstOrDefault());
                    return View(form);
                }
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Sid, r.data.UserId),
                    new Claim(ClaimTypes.Name,r.data.Email),
                    new Claim(ClaimTypes.Surname,r.data.LastName)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = form.IsRememberme,
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity)
                    , authProperties);
                //using var connection = this.rabbitMQService.CreateChannel();
                //using var model = connection.CreateModel();
                //var jsonData = JsonConvert.SerializeObject(r.data);
                //var body = Encoding.UTF8.GetBytes(jsonData);
                ////var resultProduce = await this.kafkaProducer.ProduceAsync("SignIn",
                ////    new Message<string, string> { Value = jsonData, Key = Guid.NewGuid().ToString() });
                //model.BasicPublish("UserLoginExchange", string.Empty, basicProperties: null, body: body);
                return Redirect("/User/UserManagement");
                //return View(form);
            }
            else
            {
                return View(form);
            }
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/User/SignIn");
        }
        [AllowAnonymous]
        public IActionResult Register() 
        { 
            return View();
        }
        [AllowAnonymous]
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
            return File(fileContent, System.Net.Mime.MediaTypeNames.Application.Octet, "UserManegementExport.xlsx");
        }
        [HttpPost("User/Import")]
        public async Task<IActionResult> Import()
        {
            IFormCollection formCollect = HttpContext.Request.Form;
            IFormFileCollection files = HttpContext.Request.Form.Files;
            bool IsFeedback = (formCollect["feedbackimport"].FirstOrDefault() ?? "off") == "on"?true:false;
            if(files.Count() == 0 || files.FirstOrDefault()?.Length == 0)
            {
                return Redirect("/User/UserManagement");
            }
            using (MemoryStream stream = new MemoryStream())
            {
                files[0].CopyTo(stream);
                IExcelImporter Importer = new ExcelImporter();
                var data = await Importer.Import<UserimportDto>(stream);
                //if (data.Data is null)
                //{
                //    throw new Exception("Invalid template import user management");
                //}
                IList<DataRowErrorInfo> rowErros = data.RowErrors;
                List<UserimportDto> importData = data.Data.Cast<UserimportDto>().ToList();
                foreach (var item in rowErros)
                {
                    var fieldErrors = item.FieldErrors.Select(s => s.Value);
                    //Has index header
                    importData[item.RowIndex - 2].Errors = string.Join("|", fieldErrors);
                }
                var result = await userService.ImportUser(importData, HttpContext.User.Claims.Where(s => s.Type == ClaimTypes.Name).First().Value);
                if (IsFeedback && result.Where(s => s.Errors != null).Count() > 0)
                {
                    IExcelExporter exporter = new ExcelExporter();
                    var fileContent = await exporter.ExportAsByteArray(result);
                    return File(fileContent, System.Net.Mime.MediaTypeNames.Application.Octet, "User_Maagement_import.xlsx");
                }
            }
            return View(nameof(UserManagement));
        }
        [HttpGet("User/DownloadTemplateImport")]
        public async Task<IActionResult> DownloadTemplateImport()
        {
            var exportData = new List<UserimportTemplateDto>();
            IExporter exporter = new ExcelExporter();
            var fileContent = await exporter.ExportAsByteArray(exportData);
            return File(fileContent, System.Net.Mime.MediaTypeNames.Application.Octet, "Template_User_Maagement_import.xlsx");
        }
    }
}