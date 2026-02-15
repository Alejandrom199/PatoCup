using AutoMapper;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Application.Interfaces.Services.Competition;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Interfaces.Repositories.Competition;
using System.Collections.Generic;

namespace PatoCup.Application.UseCases.Competition
{
    public class PhaseService : IPhaseService
    {
        private readonly IPhaseRepository _repository;
        private readonly IMapper _mapper;

        public PhaseService(IPhaseRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PhaseResponseDto>> GetAllPhases()
        {
            var entities = await _repository.GetAllPhasesAsync();

            var mappedEntities = _mapper.Map<IEnumerable<PhaseResponseDto>>(entities);

            return mappedEntities;
        }

        public async Task<PhaseResponseDto?> GetPhaseById(int id)
        {
            var entity =  await _repository.GetPhaseByIdAsync(id);

            var mappedEntity = _mapper.Map<PhaseResponseDto?>(entity);

            return entity == null ? null : mappedEntity;
        }

        public async Task<int> CreatePhase(CreatePhaseDto phaseDto)
        {
            var entity = _mapper.Map<Phase>(phaseDto);

            return await _repository.CreatePhaseAsync(entity);
        }

        public async Task<bool> UpdatePhase(UpdatePhaseDto phaseDto)
        {
            var entity = _mapper.Map<Phase>(phaseDto);

            return await _repository.UpdatePhaseAsync(entity);

        }
        public async Task<bool> SoftDeletePhase(int id) => await _repository.SoftDeletePhaseAsync(id);
        
        public async Task<bool> ReactivatePhase(int id) => await _repository.ReactivatePhaseAsync(id);

        public async Task<IEnumerable<PhaseResponseDto>> GetPhasesByTournamentId(int tournamentId)
        {
            var entities = await _repository.GetPhasesByTournamentIdAsync(tournamentId);

            var mappedEntities = _mapper.Map<IEnumerable<PhaseResponseDto>>(entities);

            return mappedEntities;
        }

        public async Task<bool> SetFinalPhase(int tournamentId, int phaseId) => await _repository.SetFinalPhaseAsync(tournamentId, phaseId);
    }
}
