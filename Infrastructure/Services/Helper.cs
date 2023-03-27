using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Infrastructure.Services
{
    public partial class Helper : IHelper
    {
        private readonly IConfiguration configuration;
        public Helper(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string GenerateHash(string password)
        {
            var saltConf = configuration.GetSection("salt").Value?? "saltconfigurationrandomstring";
            byte[] salt = Encoding.ASCII.GetBytes(saltConf);
            var PasswordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
               password: saltConf + "." + password + ".AppBroker",
               salt: salt,
               prf: KeyDerivationPrf.HMACSHA512,
               iterationCount: 10000,
               numBytesRequested: 128));
            return PasswordHash;
        }
    }
}
