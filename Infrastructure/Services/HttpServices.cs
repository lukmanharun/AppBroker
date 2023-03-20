using Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class HttpServices : IHttpServices
    {
        private readonly HttpClient _httpClient;
        public HttpServices(HttpClient _httpClient)
        {
            this._httpClient = _httpClient;
        }
    }
}
