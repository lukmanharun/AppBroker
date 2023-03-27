using Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;

namespace BusinessCore
{
    public class CounterService : ICounterService
    {
        private readonly IRepositoryService repositoryService;
        public CounterService(IRepositoryService repositoryService)
        {
            this.repositoryService = repositoryService;
        }
        public async Task<(string value, string error)> GenerateCounterCodeAsync(string CounterCode, int Year, int Month, string Author)
        {
            var result = new SqlParameter()
            {
                ParameterName = "result",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Direction = System.Data.ParameterDirection.Output,
                Size = int.MaxValue
            };
            var ErrorMessage = new SqlParameter()
            {
                ParameterName = "ErrorMessage",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Direction = System.Data.ParameterDirection.Output,
                Size = int.MaxValue
            };
            var rs = await repositoryService.ExecuteStoreProcedure("exec @return_value = dbo.SpCounterNumber @ParamCounterCode,@Year,@month,@Author,@result OUTPUT,@ErrorMessage OUTPUT",
                new SqlParameter("@ParamCounterCode", CounterCode),
                new SqlParameter("@Year", Year),
                new SqlParameter("@month", Month),
                new SqlParameter("@Author", Author),
                new SqlParameter()
                {
                    ParameterName = "return_value",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                },
                result, ErrorMessage);
            return (result?.Value?.ToString() ?? "", ErrorMessage?.Value?.ToString() ?? "");
        }
    }
}
