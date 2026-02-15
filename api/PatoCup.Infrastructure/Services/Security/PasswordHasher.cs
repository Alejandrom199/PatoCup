using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using PatoCup.Application.Interfaces.Services.Security;

namespace PatoCup.Infrastructure.Services.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string passwordHash, string inputPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(inputPassword, passwordHash);
            }
            catch
            {
                return false;
            }
        }
    }
}