using AppBroker.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace AppBroker.Services
{
    public class HelperService :IHelperService
    {
        private readonly IConfiguration configuration;
        public HelperService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task UploadFile(IFormFileCollection files)
        {
            var folder = configuration.GetSection("FileManagement").GetSection("Folder").Value ?? "Resources";
            long size = files.Sum(f => f.Length);
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = $"{Directory.GetCurrentDirectory()}/{folder}/{formFile.FileName}";
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

        }
    }
}
