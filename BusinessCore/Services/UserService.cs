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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        public async Task<List<UserListDTO>> GridListUserQueryrable(IFormCollection form)
        {
            var data = await dbcontext.AspNetUsers.DataTableGridAsQueryrable<AspNetUser>(form).AsNoTracking().ToListAsync();
            var result = mapper.Map<List<UserListDTO>>(data);
            return result;
        }
        public async Task<AspNetUser> GetUserByUserId(string UserId)
        {
            var data = await dbcontext.AspNetUsers.Where(s=>s.UserId == UserId).AsNoTracking()
                .FirstOrDefaultAsync();
            if (data == null) return new AspNetUser();
            var response = mapper.Map<AspNetUser>(data);
            return response;
        }

        public async Task<List<AspNetUser>> ExportUserManagement()
        {
            return await dbcontext.AspNetUsers.AsNoTracking().ToListAsync();
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
        public async Task<string> LoginAsync(SignInDTO form)
        {
            var data = await dbcontext.AspNetUsers.Where(s=>s.Email == form.Email).FirstOrDefaultAsync();
            if(data == null)
            {
                throw new Exception("User not found");
            }
            if(data.PasswordHash != helper.GenerateHash(form.Password))
            {
                throw new Exception("Password not match");
            }
            var issuer = configuration.GetSection("JwtSetting").GetSection("IsUser").Value??"AppBroker";
            var audience = configuration.GetSection("JwtSetting").GetSection("Audience").Value ?? "AppBroker";
            var secretKey = configuration.GetSection("JwtSetting").GetSection("SecretKey").Value ?? "AppBroker";
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, data.Email),
                    new Claim(JwtRegisteredClaimNames.Email, data.Email),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
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

    }
}
