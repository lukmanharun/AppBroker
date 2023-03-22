using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace AppBroker.ViewComponentBase
{
    public sealed class ButtonAddNewModel
    {
        public string href { get; set; } = "#";
        public string name { get; set; } = "Add New";
    }

    [ViewComponent(Name = "ButtonAddNew")]
    public class ButtonAddNewViewComponent : ViewComponent
    {
        public ButtonAddNewViewComponent()
        {
        }
        public async Task<IViewComponentResult> InvokeAsync(string? href)
        {
            var model = new ButtonAddNewModel();
            model.href = href?? "#";
            return View("Index", model);
        }

    }
}
