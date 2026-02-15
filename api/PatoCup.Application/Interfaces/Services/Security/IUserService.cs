using PatoCup.Application.DTOs.Security;
using PatoCup.Domain.Entities.Security;
using System.Threading.Tasks;

namespace PatoCup.Application.Interfaces.Services.Security
{
    public interface IUserService
    {
        Task<int> CreateUserAsync(CreateUserDto request);
        Task<bool> UpdateAsync(UpdateUserDto user);
        Task<UserResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<UserResponseDto>> GetAllAsync(int pageNumber, int pageSize, string? filterUsername, int? filterRoleId);
        Task<bool> SoftDeleteAsync(int id);
    }
}