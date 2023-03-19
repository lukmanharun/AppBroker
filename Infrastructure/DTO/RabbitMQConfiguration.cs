using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public class RabbitMQConfiguration
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string HostName { get; set; }
    }
}
