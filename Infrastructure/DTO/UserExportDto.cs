using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    [ExcelExporter(Name = "UserManagementImport")]
    public class UserimportTemplateDto
    {
        [ExporterHeader(DisplayName = "FirstName")]
        public string FirstName { get; set; } = null!;
        [ExporterHeader(DisplayName = "LastName")]
        public string LastName { get; set; } = null!;
        [ExporterHeader(DisplayName = "Email")]
        public string Email { get; set; } = null!;
        [ExporterHeader(DisplayName = "Password")]
        public string PasswordHash { get; set; } = null!;
    }
    [ExcelImporter(IsLabelingError = true,SheetName ="User Management")]
    public class UserimportDto
    {
        [ExporterHeader(DisplayName = "FirstName")]
        [ImporterHeader(Name = "FirstName")]
        [Required]
        public string FirstName { get; set; } = null!;
        [ExporterHeader(DisplayName = "LastName")]
        [ImporterHeader(Name = "LastName")]
        [Required]
        public string LastName { get; set; } = null!;
        [ExporterHeader(DisplayName = "Email")]
        [ImporterHeader(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Required]
        public string Email { get; set; } = null!;
        [ExporterHeader(DisplayName = "Password")]
        [ImporterHeader(Name = "Password")]
        [Required]
        [MinLength(8, ErrorMessage = "Password minimal 8 chacrater")]
        public string PasswordHash { get; set; } = null!;
        [ExporterHeader(DisplayName = "Errors")]
        [ImporterHeader(Name = "Errors",IsIgnore =true)]
        public string? Errors { get; set; }
    }
}
