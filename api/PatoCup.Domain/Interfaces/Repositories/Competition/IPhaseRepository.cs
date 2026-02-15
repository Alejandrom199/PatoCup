using PatoCup.Domain.Entities.Competition;

namespace PatoCup.Domain.Interfaces.Repositories.Competition
{
    public interface IPhaseRepository
    {
        Task<IEnumerable<Phase>> GetAllPhasesAsync();
        Task<Phase?> GetPhaseByIdAsync(int id);
        Task<int> CreatePhaseAsync(Phase phase);
        Task<bool> UpdatePhaseAsync(Phase phase);
        Task<bool> SoftDeletePhaseAsync(int id);
        Task<bool> ReactivatePhaseAsync(int id);
        Task<IEnumerable<Phase>> GetPhasesByTournamentIdAsync(int tournamentId);
        Task<bool> SetFinalPhaseAsync(int tournamentId, int phaseId);
    }
}
