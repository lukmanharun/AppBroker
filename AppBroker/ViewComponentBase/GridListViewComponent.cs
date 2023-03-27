using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Infrastructure.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace AppBroker.ViewComponentBase
{
    public sealed class GridListModel
    {
        public required GridColumnModel[] Columns { get; set; }
        public required JArray Data { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public decimal PageLength { get; set; }
        public decimal RecordPagination { get; set; }
    }
    public sealed class GridColumnModel
    {
        public required string Column { get; set; }
        public required string DisplayName { get; set; }
        public bool IsAllowSort { get; set; } = true;
        public bool IsAllowSearch { get; set; } = true;
        public bool IsHidden { get; set; } = false;
    }
    [ViewComponent(Name ="GridList")]
    public class GridListViewComponent : ViewComponent
    {
        private readonly AppDbContext dbContext;
        public GridListViewComponent(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var data = await dbContext.AspNetUsers.Select(s=> new
            {
                UserId = s.UserId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                CreatedAt = s.CreatedAt
            }).AsNoTracking().Skip(0).Take(5).ToListAsync();
            var datacount = await dbContext.AspNetUsers.CountAsync();
            string jsonData = JsonConvert.SerializeObject(data);

            JArray parsedArray = (JArray)JsonConvert.DeserializeObject(jsonData);
            var columns = new GridColumnModel[]
            {
                new GridColumnModel
                {
                    Column = "UserId",
                    DisplayName = "UserId",
                    IsAllowSearch = false,
                    IsAllowSort = false,
                    IsHidden = true,
                },
                new GridColumnModel
                {
                    Column = "FirstName",
                    DisplayName = "First Name",
                    IsAllowSearch = true,
                    IsAllowSort = true,
                    IsHidden = false,
                },
                new GridColumnModel
                {
                    Column = "LastName",
                    DisplayName = "Last Name",
                    IsAllowSearch = true,
                    IsAllowSort = true,
                    IsHidden = false,
                },
                new GridColumnModel
                {
                    Column = "Email",
                    DisplayName = "Email",
                    IsAllowSearch = true,
                    IsAllowSort = true,
                    IsHidden = false,
                }
                ,new GridColumnModel
                {
                    Column = "CreatedAt",
                    DisplayName = "Created",
                    IsAllowSearch = true,
                    IsAllowSort = true,
                    IsHidden = false,
                }
            };
            decimal PageLength = 5;
            GridListModel gridList = new GridListModel()
            { 
                Data = parsedArray,
                Columns = columns,
                PageLength = PageLength,
                RecordsTotal = datacount,
                RecordsFiltered = datacount,
                RecordPagination = Math.Round((datacount / PageLength),MidpointRounding.AwayFromZero)
            };
            return View("Index",gridList);
        }
    }
}
