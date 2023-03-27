using Infrastructure.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public sealed class SalesOrderHeaderNewDTO
    {
        public string SalesOrderCode { get; set; } = null!;

        public DateTime SalesOrderDate { get; set; }
    }
}
