using AutoMapper;
using BusinessCore.Interfaces;
using Infrastructure;
using Infrastructure.Data;
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
        private readonly AppDbContext dbcontext;
        private readonly IMapper mapper;
        private readonly IHelper helper;
        private readonly IConfiguration configuration;
        public UserService(AppDbContext dbcontext,IMapper mapper, IHelper helper, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.helper = helper;
            this.dbcontext = dbcontext;
            this.mapper = mapper;
        }
        public async Task<GridDataTable<UserListDTO>> GridListUserQueryrable(IFormCollection form)
        {
            return await dbcontext.AspNetUsers.Select(s => new UserListDTO
            {
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                UserId = s.UserId
            }).AsNoTracking().DataTableGrid<UserListDTO>(form);
            
        }
        public async Task<AspNetUser> GetUserByUserId(string UserId)
        {
            var data = await dbcontext.AspNetUsers.Where(s=>s.UserId == UserId).AsNoTracking()
                .FirstOrDefaultAsync();
            if (data == null) return new AspNetUser();
            var response = mapper.Map<AspNetUser>(data);
            return response;
        }

        public async Task<List<UserimportDto>> ImportUser(List<UserimportDto> data,string userid)
        {
            var emails = data.Select(s => s.Email);
            var emailExist = await dbcontext.AspNetUsers.Where(s => emails.Contains(s.Email)).AsNoTracking().ToListAsync();
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
            await dbcontext.AspNetUsers.AddRangeAsync(dataImport);
            await dbcontext.SaveChangesAsync();
            return data;
        }
        public async Task<List<AspNetUser>> ExportUserManagement()
        {
            return await dbcontext.AspNetUsers.Select(s=> new AspNetUser
            {
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
            }).AsNoTracking().ToListAsync();
        }
        public async Task DeleteUser(string userid)
        {
            var data = await dbcontext.AspNetUsers.Where(s => s.UserId == userid).AsNoTracking().FirstOrDefaultAsync();
            if (data == null)
            {
                throw new Exception("User not found");
            }
            dbcontext.AspNetUsers.Remove(data);
            await dbcontext.SaveChangesAsync();
        }
        public async Task EditUser(UserEditSubmitDTO form, string userid)
        {
            var data = await dbcontext.AspNetUsers.Where(s => s.UserId == form.UserId).AsNoTracking().FirstOrDefaultAsync();
            if (data == null)
            {
                throw new Exception("User not found");
            }
            data.FirstName = form.FirstName;
            data.LastName = form.LastName;
            data.Email = form.Email;
            data.ModifiedAt = DateTime.Now;
            data.ModifiedBy = userid;
            dbcontext.AspNetUsers.Update(data);
            await dbcontext.SaveChangesAsync();
        }
        public async Task AddNewUser(RegisterDTO form,string userid)
        {
            var data = mapper.Map<AspNetUser>(form);
            var isExist = await dbcontext.AspNetUsers.Where(s => s.Email == data.Email).AsNoTracking().AnyAsync();
            if (isExist)
            {
                throw new Exception("Email is Exist");
            }
            data.CreatedAt = DateTime.Now;
            data.CreatedBy = userid;
            data.UserId = Guid.NewGuid().ToString();
            data.PasswordHash = helper.GenerateHash(form.Password);
            await dbcontext.AspNetUsers.AddAsync(data);
            await dbcontext.SaveChangesAsync();
        }
        public async Task RegisterAsync(RegisterDTO form)
        {
            var data = mapper.Map<AspNetUser>(form);
            var isExist = await dbcontext.AspNetUsers.Where(s => s.Email == data.Email).AnyAsync();
            if(isExist) 
            {
                throw new Exception("Email is Exist");
            }
            data.CreatedAt = DateTime.Now;
            data.CreatedBy = "Guest";
            data.UserId = Guid.NewGuid().ToString();
            data.PasswordHash = helper.GenerateHash(form.Password);
            await dbcontext.AspNetUsers.AddAsync(data);
            await dbcontext.SaveChangesAsync();
        }
        public async Task<(bool success,AspNetUser data,Dictionary<string, string> errors)> Authentication(SignInDTO form)
        {
            form.Password = helper.GenerateHash(form.Password);
            var data = await dbcontext.AspNetUsers.Where(s => s.Email == form.Email )
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
