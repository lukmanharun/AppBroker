using AutoMapper;
using Infrastructure;
using Infrastructure.Entity;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BusinessCore.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper mapper;
        private readonly IHelper helper;
        private readonly IConfiguration configuration;
        private readonly IRepositoryService repositoryService;
        public UserService(IMapper mapper, IHelper helper, IConfiguration configuration, IRepositoryService repositoryService)
        {
            this.configuration = configuration;
            this.helper = helper;
            this.mapper = mapper;
            this.repositoryService = repositoryService;
        }

        public async Task<GridDataTable<UserListDTO>> GridListUserQueryrable(IFormCollection form)
        {
            return await repositoryService.Queryrable<AspNetUser>().Select(s => new UserListDTO
            {
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                UserId = s.UserId,
                CreatedAt = s.CreatedAt,
                CreatedAtFormat = s.CreatedAt.Format(),
            }).AsNoTracking().DataTableGridServerSide<UserListDTO>(form, IgnorePropertySearch: "UserId"
            ,TransformProperty:new Dictionary<string, string>
            {
                {"CreatedAtFormat","CreatedAt"}
            });
        }
        public async Task<AspNetUser> GetUserByUserId(string UserId)
        {
            var data = await repositoryService.Queryrable<AspNetUser>().Where(s=>s.UserId == UserId).AsNoTracking()
                .FirstOrDefaultAsync();
            if (data == null) return new AspNetUser();
            var response = mapper.Map<AspNetUser>(data);
            return response;
        }

        public async Task<List<UserimportDto>> ImportUser(List<UserimportDto> data,string userid)
        {
            var emails = data.Select(s => s.Email);
            var emailExist = await repositoryService.Queryrable<AspNetUser>().Where(s => emails.Contains(s.Email)).AsNoTracking().ToListAsync();
            if(emailExist.Count()>0)
            {
                foreach (var item in emailExist)
                {
                    int index = data.IndexOf(data.Where(s => s.Email == item.Email).First());
                    data[index].Errors = $"{data[index].Errors}|Email is exist";
                }
            }
            IEnumerable<UserimportDto> inData = data.Where(s => !emailExist.Any(d => d.Email == s.Email) && s.Errors == null);
            var dataImport = mapper.Map<List<AspNetUser>>(inData);
            dataImport.ForEach(s =>
            {
                s.CreatedAt = DateTime.Now;
                s.CreatedBy = userid;
                s.PasswordHash = helper.GenerateHash(s.PasswordHash);
                s.UserId = Guid.NewGuid().ToString();
            });
            await repositoryService.AddRangeAsync<AspNetUser>(dataImport);
            await repositoryService.SaveChangesAsync();
            return data;
        }
        public async Task<List<AspNetUser>> ExportUserManagement()
        {
            return await repositoryService.Queryrable<AspNetUser>().Select(s=> new AspNetUser
            {
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
            }).AsNoTracking().ToListAsync();
        }
        public async Task DeleteUser(string userid)
        {
            var data = await repositoryService.Queryrable<AspNetUser>().Where(s => s.UserId == userid).AsNoTracking().FirstOrDefaultAsync();
            if (data == null)
            {
                throw new Exception("User not found");
            }
            repositoryService.Remove<AspNetUser>(data);
            await repositoryService.SaveChangesAsync();
        }
        public async Task EditUser(UserEditSubmitDTO form, string userid)
        {
            var data = await repositoryService.Queryrable<AspNetUser>().Where(s => s.UserId == form.UserId).AsNoTracking().FirstOrDefaultAsync();
            if (data == null)
            {
                throw new Exception("User not found");
            }
            data.FirstName = form.FirstName;
            data.LastName = form.LastName;
            data.Email = form.Email;
            data.ModifiedAt = DateTime.Now;
            data.ModifiedBy = userid;
            repositoryService.Update<AspNetUser>(data);
            await repositoryService.SaveChangesAsync();
        }
        public async Task AddNewUser(RegisterDTO form,string userid)
        {
            var data = mapper.Map<AspNetUser>(form);
            var isExist = await repositoryService.Queryrable<AspNetUser>().Where(s => s.Email == data.Email).AsNoTracking().AnyAsync();
            if (isExist)
            {
                throw new Exception("Email is Exist");
            }
            data.CreatedAt = DateTime.Now;
            data.CreatedBy = userid;
            data.UserId = Guid.NewGuid().ToString();
            data.PasswordHash = helper.GenerateHash(form.Password);
            await repositoryService.AddAsync(data);
            await repositoryService.SaveChangesAsync();
        }
        public async Task RegisterAsync(RegisterDTO form)
        {
            var data = mapper.Map<AspNetUser>(form);
            var isExist = await repositoryService.Queryrable<AspNetUser>().Where(s => s.Email == data.Email).AnyAsync();
            if(isExist) 
            {
                throw new Exception("Email is Exist");
            }
            data.CreatedAt = DateTime.Now;
            data.CreatedBy = "Guest";
            data.UserId = Guid.NewGuid().ToString();
            data.PasswordHash = helper.GenerateHash(form.Password);
            await repositoryService.AddAsync(data);
            await repositoryService.SaveChangesAsync();
        }
        public async Task<(bool success,AspNetUser data,Dictionary<string, string> errors)> Authentication(SignInDTO form)
        {
            form.Password = helper.GenerateHash(form.Password);
            var data = await repositoryService.Queryrable<AspNetUser>().Where(s => s.Email == form.Email )
                .FirstOrDefaultAsync();
            if (data == null)
            {
                return (false,new AspNetUser(), new Dictionary<string, string>
                {
                    {
                        "Email","Email dosent exist"
                    }
                });
            }
            if(data.PasswordHash != form.Password)
            {
                return (false, new AspNetUser(), new Dictionary<string, string>
                {
                    {
                        "Password","Invalid password"
                    }
                });
            }
            return (true,data, new Dictionary<string, string>());
        }

    }
}
