using PatoCup.Domain.Entities.Security;

namespace PatoCup.Application.Interfaces.Services.Security
{
    public interface IJwtGenerator
    {
        string GenerateToken(User user);
    }
}