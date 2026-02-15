using AutoMapper;
using PatoCup.Application.DTOs.Common;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Application.Interfaces.Services.Common;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Interfaces.Repositories.Common;

namespace PatoCup.Application.UseCases.Common
{
    public class CatalogService : ICatalogService
    {

        private readonly ICatalogRepository _repository;
        private readonly IMapper _mapper;
        

        public CatalogService(ICatalogRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;

        }

        public async Task<IEnumerable<CatalogDto>> GeneralStatusGetAll()
        {
            var catalog = await _repository.GeneralStatusGetAllAsync();
            return _mapper.Map<IEnumerable<CatalogDto>>(catalog);
        }

        public async Task<IEnumerable<CatalogDto>> MatchStatusGetAll()
        {
            var catalog = await _repository.MatchStatusGetAllAsync();
            return _mapper.Map<IEnumerable<CatalogDto>>(catalog);
        }

        public async Task<IEnumerable<CatalogDto>> PhaseStatusGetAll()
        {
            var catalog = await _repository.PhaseStatusGetAllAsync();
            return _mapper.Map<IEnumerable<CatalogDto>>(catalog);
        }

        public async Task<IEnumerable<CatalogDto>> PlayerStatusGetAll()
        {
            var catalog = await _repository.PlayerStatusGetAllAsync();
            return _mapper.Map<IEnumerable<CatalogDto>>(catalog);
        }

        public async Task<IEnumerable<CatalogDto>> TournamentStatusGetAll()
        {
            var catalog = await _repository.TournamentStatusGetAllAsync();
            return _mapper.Map<IEnumerable<CatalogDto>>(catalog);
        }
    }
}
