using Infrastructure;
using Infrastructure.DTO;
using Infrastructure.Entity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessCore
{
    public interface ISalesOrderService
    {
        Task AddSalesOrderHeader(DateTime SalesOrderDate, string UserId);
        Task<List<SalesOrderDt>> GetSalesOrderDtByCode(string SalesOrderCode);
        Task<SalesOrderDt?> GetSalesOrderDtById(long Id);
        Task<SalesOrderHd?> GetSalesOrderHdByCode(string SalesOrderCode);
        Task<GridDataTable<GridListSalesOrderHd>> GridListSalesOrderHeader(IFormCollection form);
    }
}
