using AutoMapper;
using PatoCup.Application.DTOs.Security;
using PatoCup.Application.Interfaces.Services.Security;
using PatoCup.Domain.Interfaces.Repositories.Security;

namespace PatoCup.Application.UseCases.Security
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _repository;
        private readonly IMapper _mapper;

        public MenuService(IMenuRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MenuResponseDto>> GetMenuByUser(int userId)
        {
            
            var entity = await _repository.GetMenuByUserAsync(userId);

            return _mapper.Map<IEnumerable<MenuResponseDto>>(entity);

        }

    }
}
