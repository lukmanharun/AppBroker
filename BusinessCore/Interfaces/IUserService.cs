using Infrastructure;
using Infrastructure.Entity;
using Microsoft.AspNetCore.Http;

namespace BusinessCore.Interfaces
{
    public interface IUserService
    {
        Task AddNewUser(RegisterDTO form, string userid);
        Task DeleteUser(string userid);
        Task EditUser(UserEditSubmitDTO form, string userid);
        Task<List<AspNetUser>> ExportUserManagement();
        Task<AspNetUser> GetUserByUserId(string UserId);
        Task<List<UserListDTO>> GridListUserQueryrable(IFormCollection form);
        Task<string> LoginAsync(SignInDTO form);
        Task RegisterAsync(RegisterDTO form);
    }
}
