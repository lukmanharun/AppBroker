using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessCore
{
    public interface ICounterService
    {
        Task<(string value, string error)> GenerateCounterCodeAsync(string CounterCode, int Year, int Month, string Author);
    }
}
