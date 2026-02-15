using PatoCup.Application.DTOs.Competition;
using PatoCup.Domain.Entities.Competition;

namespace PatoCup.Application.Interfaces.Services.Competition
{
    public interface IPhaseService
    {
        Task<IEnumerable<PhaseResponseDto>> GetAllPhases();
        Task<PhaseResponseDto?> GetPhaseById(int id);
        Task<int> CreatePhase(CreatePhaseDto phaseDto);
        Task<bool> UpdatePhase(UpdatePhaseDto phaseDto);
        Task<bool> SoftDeletePhase(int id);
        Task<bool> ReactivatePhase(int id);
        Task<IEnumerable<PhaseResponseDto>> GetPhasesByTournamentId(int tournamentId);
        Task<bool> SetFinalPhase(int tournamentId, int phaseId);
    }
}



