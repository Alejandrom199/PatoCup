using PatoCup.Domain.Entities.Competition;

namespace PatoCup.Domain.Interfaces.Repositories.Competition
{
    public interface ITournamentRepository
    {
        Task<IEnumerable<Tournament>> GetAllTournamentsAsync(int pageNumber, int pageSize, Tournament tournament);
        Task<Tournament?> GetTournamentByIdAsync(int id);
        Task<int> CreateTournamentAsync(Tournament tournament);
        Task<bool> UpdateTournamentAsync(Tournament tournament);
        Task<bool> SoftDeleteTournamentAsync(int id);
        Task<bool> ReactivateTournamentAsync(int id);
        Task<bool> SetPublicTournamentAsync(int id);
        Task<Tournament?> GetPublicActiveTournamentAsync();
        Task<Tournament?> GetPublicBracketAsync();
    }
}