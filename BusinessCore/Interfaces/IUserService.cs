using Infrastructure;
using Infrastructure.Entity;
using Magicodes.ExporterAndImporter.Core.Models;
using Microsoft.AspNetCore.Http;

namespace BusinessCore.Interfaces
{
    public interface IUserService
    {
        Task AddNewUser(RegisterDTO form, string userid);
        Task<(bool success, AspNetUser data, Dictionary<string, string> errors)> Authentication(SignInDTO form);
        Task DeleteUser(string userid);
        Task EditUser(UserEditSubmitDTO form, string userid);
        Task<List<AspNetUser>> ExportUserManagement();
        Task<AspNetUser> GetUserByUserId(string UserId);
        Task<GridDataTable<UserListDTO>> GridListUserQueryrable(IFormCollection form);
        Task<List<UserimportDto>> ImportUser(List<UserimportDto> data, string userid);
        Task RegisterAsync(RegisterDTO form);
    }
}
