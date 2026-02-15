using AutoMapper;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Application.Interfaces.Services.Competition;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Interfaces.Repositories.Competition;

namespace PatoCup.Application.UseCases.Competition
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _repository;
        private readonly IMapper _mapper;

        public MatchService(IMatchRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<int> CreateMatch(CreateMatchDto dto)
        {
            var entity = _mapper.Map<Match>(dto);
            return await _repository.CreateMatchAsync(entity);
        }

        public async Task<bool> UpdateMatch(UpdateMatchDto dto)
        {
            var entity = _mapper.Map<Match>(dto);
            return await _repository.UpdateMatchAsync(entity);
        }

        public async Task<bool> RegisterResult(RegisterMatchResultDto dto)
        {
            var entity = _mapper.Map<Match>(dto);
            return await _repository.RegisterResultAsync(entity);
        }

        public async Task<IEnumerable<MatchResponseDto>> GetMatchesByPhaseId(int phaseId)
        {
            var entities = await _repository.GetMatchesByPhaseIdAsync(phaseId);
            return _mapper.Map<IEnumerable<MatchResponseDto>>(entities);
        }

        public async Task<bool> SoftDeleteMatch(int id)
        {
            return await _repository.SoftDeleteMatchAsync(id);
        }
    }
}