using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    [ExcelExporter(Name = "User Management",  AutoFitAllColumn = true)]
    public class UserExportDto
    {
        [ExporterHeader(DisplayName = "First Name")]

        public string FirstName { get; set; } = null!;
        [ExporterHeader(DisplayName = "Last Name")]

        public string LastName { get; set; } = null!;
        [ExporterHeader(DisplayName = "Email")]

        public string Email { get; set; } = null!;
    }
}
