using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Entities.Security;
using System.Threading.Tasks;

namespace PatoCup.Domain.Interfaces.Repositories.Security
{
    public interface IUserRepository
    {
        Task<int> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize, string? filterUsername, int? filterRoleId);
        Task<bool> SoftDeleteAsync(int id);
    }
}