using System;
using System.Threading.Tasks;
using PatoCup.Application.DTOs.Auth;
using PatoCup.Application.Interfaces.Services.Security;
using PatoCup.Domain.Interfaces.Repositories.Security;

namespace PatoCup.Application.UseCases.Security
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtGenerator _jwtGenerator;

        public AuthService(
            IAuthRepository authRepository,
            IPasswordHasher passwordHasher,
            IJwtGenerator jwtGenerator)
        {
            _authRepository = authRepository;
            _passwordHasher = passwordHasher;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _authRepository.LoginAsync(request.Username);


            bool esPasswordValido = _passwordHasher.Verify(user.Password, request.Password);

            if (!esPasswordValido)
            {
                throw new Exception("La contraseña ingresada es incorrecta.");
            }

            var token = _jwtGenerator.GenerateToken(user);

            return new LoginResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.RoleName,
                PhotoUrl = user.PhotoUrl,
                Token = token
            };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            string newPasswordHash = _passwordHasher.Hash(dto.NewPassword);

            return await _authRepository.ChangePassword(dto.UserId, newPasswordHash);
        }
    }
}