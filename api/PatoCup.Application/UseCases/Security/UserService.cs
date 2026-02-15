using AutoMapper;
using FluentValidation;
using PatoCup.Application.DTOs.Security;
using PatoCup.Application.Interfaces.Services.Security;
using PatoCup.Domain.Entities.Security;
using PatoCup.Domain.Interfaces.Repositories.Security;
using System.Threading.Tasks;

namespace PatoCup.Application.UseCases.Security
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<CreateUserDto> _validator;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository repository,
            IPasswordHasher passwordHasher,
            IValidator<CreateUserDto> validator,
            IMapper mapper)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<int> CreateUserAsync(CreateUserDto request)
        {

            string passwordHash = _passwordHasher.Hash(request.Password);

            var user = _mapper.Map<User>(request);
            user.Password = passwordHash;

            return await _repository.CreateAsync(user);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllAsync(int pageNumber, int pageSize, string? filterUsername, int? filterRoleId)
        {
            var users = await _repository.GetAllAsync(pageNumber, pageSize, filterUsername, filterRoleId);

            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        public async Task<UserResponseDto?> GetByIdAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id);

            return _mapper.Map<UserResponseDto?>(user);
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            return await _repository.SoftDeleteAsync(id);
        }

        public async Task<bool> UpdateAsync(UpdateUserDto request)
        {

            var user = _mapper.Map<User>(request);

            return await _repository.UpdateAsync(user);
        }
    }
}