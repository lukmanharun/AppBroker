using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public partial interface IHelper
    {
        string GenerateHash(string password);
    }
}
