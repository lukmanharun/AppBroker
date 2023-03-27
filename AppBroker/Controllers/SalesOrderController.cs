
using AppBroker.Models;
using BusinessCore;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppBroker.Controllers
{
    public class SalesOrderController : Controller
    {
        private readonly AppDbContext dbContext;
        private readonly ISalesOrderService salesOrderService;
        public SalesOrderController(AppDbContext dbContext, ISalesOrderService salesOrderService)
        {
            this.salesOrderService = salesOrderService;
            this.dbContext = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("SalesOrder/GetGridListHeader")]
        [Produces("application/json")]
        public async Task<IActionResult> GetGridUser()
        {
            var form = HttpContext.Request.Form;
            var data = await salesOrderService.GridListSalesOrderHeader(form);
            var result = new JsonResult(data);
            return result;
        }
        public IActionResult AddNewSalesOrderHd()
        {
            return View();
        }
        [HttpPost("SalesOrder/AddNewSalesOrderHd")]
        public async Task<IActionResult> AddNewSalesOrderHdAsync(SalesOrderHdModel model)
        {
            if(!ModelState.IsValid) return View(model);
            await salesOrderService.AddSalesOrderHeader(model.SalesOrderDate, HttpContext.User.Claims.Where(s=>s.Type == ClaimTypes.Sid).First().Value);
            return Redirect("/SalesOrder/Index");
        }
    }
}
