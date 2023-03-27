using Infrastructure.Entity;
using Infrastructure;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Services;
using Infrastructure.DTO;
using AutoMapper;
namespace BusinessCore
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IRepositoryService repositoryService;
        private readonly ICounterService counterService;
        private readonly IMapper mapper;
        public SalesOrderService(IRepositoryService repositoryService, IMapper mapper, ICounterService counterService)
        {
            this.counterService = counterService;
            this.repositoryService = repositoryService;
            this.mapper = mapper;

        }
        public async Task<GridDataTable<GridListSalesOrderHd>> GridListSalesOrderHeader(IFormCollection form)
        {
            return await repositoryService.Queryrable<SalesOrderHd>().Include(s=>s.SalesOrderDts).Select(s => new GridListSalesOrderHd
            {
                SalesOrderCode = s.SalesOrderCode,
                SalesOrderDate = s.SalesOrderDate,
                SalesOrderDateFormat = s.SalesOrderDate.Format(),
                StatusCode = s.StatusCode,
                TotalAmount = s.SalesOrderDts.Sum(s=>s.Amount)
            }).AsNoTracking().DataTableGridServerSide<GridListSalesOrderHd>(form, IgnorePropertySearch: "TotalAmount"
            , TransformProperty: new Dictionary<string, string>
            {
                {"SalesOrderDateFormat","SalesOrderDate"}
            });
        }
        public async Task<SalesOrderHd?> GetSalesOrderHdByCode(string SalesOrderCode)
        {
            var data = await repositoryService.Queryrable<SalesOrderHd>().Include(s=>s.SalesOrderDts)
                .Where(s => s.SalesOrderCode == SalesOrderCode)
                .FirstOrDefaultAsync();
            return data;
        }
        public async Task<List<SalesOrderDt>> GetSalesOrderDtByCode(string SalesOrderCode)
        {
            var data = await repositoryService.Queryrable<SalesOrderDt>()
                .Where(s => s.SalesOrderCode == SalesOrderCode).AsNoTracking()
                .ToListAsync();
            return data;
        }
        public async Task<SalesOrderDt?> GetSalesOrderDtById(long Id)
        {
            var data = await repositoryService.Queryrable<SalesOrderDt>()
                .Where(s => s.Id == Id)
                .FirstOrDefaultAsync();
            return data;
        }
        public async Task AddSalesOrderHeader(DateTime SalesOrderDate, string UserId)
        {
            var (value,Error) = await counterService.GenerateCounterCodeAsync("SalesOrder", SalesOrderDate.Year, SalesOrderDate.Month, UserId);
            if (string.IsNullOrEmpty(value)) throw new Exception(Error);
            await repositoryService.AddAsync<SalesOrderHd>(new SalesOrderHd
            {
                CreatedAt = DateTime.Now,
                CreatedBy = UserId,
                UserId = UserId,
                SalesOrderCode = value,
                SalesOrderDate = SalesOrderDate,
                StatusCode = "Waiting Approval"
            });
            await repositoryService.SaveChangesAsync();
        }
    }
}
