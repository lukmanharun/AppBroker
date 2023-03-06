using AppBroker.BusinessCore.Entity.DTO;
using BusinessCore.Entity.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BusinessCore.Interfaces
{
    public interface IUserService
    {
        Task<List<UserListDTO>> ListUser();
        Task<string> LoginAsync(SignInDTO form);
        Task RegisterAsync(RegisterDTO form);
    }
}
