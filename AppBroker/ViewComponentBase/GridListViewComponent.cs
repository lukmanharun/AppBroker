using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json;
using Infrastructure.Data;

namespace AppBroker.ViewComponentBase
{
    public sealed class GridListModel
    {
        public GridColumnModel[] Columns { get; set; }
        public object[] Values { get; set; }
    }
    public sealed class GridColumnModel
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string DisplayName { get; set; }
        public bool IsAllowSort { get; set; } = true;
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
            var data = await dbContext.AspNetUsers.ToListAsync();
            GridListModel grid = new GridListModel();
            string jsonData = JsonConvert.SerializeObject(data);
            if (jsonData == null) return View("Index");
            JArray parsedArray = (JArray)JsonConvert.DeserializeObject(jsonData);
            foreach (JObject parsedObject in parsedArray.Children<JObject>())
            {
                foreach (JProperty parsedProperty in parsedObject.Properties())
                {
                    string propertyName = parsedProperty.Name;
                    JValue propertyValue = (JValue)parsedProperty.Value;

                }
            }
            return View("Index");
        }
    }
}
