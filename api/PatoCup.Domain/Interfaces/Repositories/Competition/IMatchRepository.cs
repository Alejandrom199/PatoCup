using PatoCup.Domain.Entities.Competition; 

namespace PatoCup.Domain.Interfaces.Repositories.Competition
{
    public interface IMatchRepository
    {
        Task<IEnumerable<Match>> GetMatchesByPhaseIdAsync(int phaseId);
        Task<int> CreateMatchAsync(Match match);
        Task<bool> UpdateMatchAsync(Match match);
        Task<bool> RegisterResultAsync(Match match);
        Task<bool> SoftDeleteMatchAsync(int id);
    }
}