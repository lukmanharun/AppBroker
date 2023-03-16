using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class HelperExtention
    {
        public static string Format(this DateTime dateTime) => dateTime.ToString("dd MMMM yyyy");
        public static string Format(this DateTime? dateTime) => dateTime?.ToString("dd MMMM yyyy") ?? "";
    }
}
