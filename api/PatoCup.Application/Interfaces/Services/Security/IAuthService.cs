using System.Threading.Tasks;
using PatoCup.Application.DTOs.Auth;

namespace PatoCup.Application.Interfaces.Services.Security
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<bool> ChangePasswordAsync(ChangePasswordDto dto);
    }
}