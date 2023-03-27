using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public sealed class GridListSalesOrderHd
    {

        public string SalesOrderCode { get; set; } = null!;
        public DateTime SalesOrderDate { get; set; }
        public string SalesOrderDateFormat { get; set; }
        public string? StatusCode { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
