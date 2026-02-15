using PatoCup.Domain.Entities.Competition;

namespace PatoCup.Domain.Interfaces.Repositories.Competition
{
    public interface IPlayerRepository
    {
        Task<bool> PublicSubmitPlayerAsync(Player phase);
        Task<IEnumerable<Player>> GetAllPlayersAsync(int pageNumber, int pageSize, string filter);
        Task<bool> ProcessPlayerRequestAsync(int playerId, int newPlayerStateId);
        Task<bool> UpdatePlayerAsync(Player phase);
        Task<bool> SoftDeletePlayerAsync(int id);
        Task<IEnumerable<Player>> GetPlayersSelect();
    }
}
