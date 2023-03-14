using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public class RabbitMQConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
    }
}
