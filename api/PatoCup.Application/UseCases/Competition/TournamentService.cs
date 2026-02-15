using AutoMapper;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Application.Interfaces.Services.Competition;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Interfaces.Repositories.Competition;

namespace PatoCup.Application.UseCases.Competition
{
    public class TournamentService : ITournamentService
    {
        private readonly ITournamentRepository _repository;
        private readonly IMapper _mapper;

        public TournamentService(ITournamentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<TournamentResponseDto>> GetAllTournaments(TournamentQueryDto query)
        {
            var tournamentFilter = _mapper.Map<Tournament>(query);

            var entities = await _repository.GetAllTournamentsAsync(query.PageNumber, query.PageSize, tournamentFilter);

            var mappedEntities = _mapper.Map<IEnumerable<TournamentResponseDto>>(entities);

            return mappedEntities;
        }

        public async Task<TournamentResponseDto?> GetTournamentById(int id)
        {
            var entity = await _repository.GetTournamentByIdAsync(id);

            var mappedEntity = _mapper.Map<TournamentResponseDto>(entity);

            return entity == null ? null : mappedEntity;
        }

        public async Task<int> CreateTournament(CreateTournamentDto request)
        {
            var entity = _mapper.Map<Tournament>(request);
            return await _repository.CreateTournamentAsync(entity);
        }

        public async Task<bool> UpdateTournament(UpdateTournamentDto request)
        {
            var entity = _mapper.Map<Tournament>(request);
            return await _repository.UpdateTournamentAsync(entity);
        }

        public async Task<bool> SoftDeleteTournament(int id) => await _repository.SoftDeleteTournamentAsync(id);

        public async Task<bool> ReactivateTournament(int id) => await _repository.ReactivateTournamentAsync(id);
        
        public async Task<bool> SetPublicTournament(int id)
        {
            var isSuccess = await _repository.SetPublicTournamentAsync(id);
            return isSuccess;
        }

        public async Task<TournamentResponseDto?> GetPublicActiveTournament()
        {
            var entity = await _repository.GetPublicActiveTournamentAsync();

            var entityMapped = _mapper.Map<TournamentResponseDto>(entity);

            return entityMapped;

        }

        public async Task<TournamentBracketDto?> GetPublicBracket()
        {
            var tournamentEntity = await _repository.GetPublicBracketAsync();

            if (tournamentEntity == null) return null;

            return _mapper.Map<TournamentBracketDto>(tournamentEntity);
        }
    }
}