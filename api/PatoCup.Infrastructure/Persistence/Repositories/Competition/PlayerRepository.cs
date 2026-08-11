using Dapper;
using PatoCup.Application.Exceptions;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Interfaces.Repositories.Competition;
using System.Data;

namespace PatoCup.Infrastructure.Persistence.Repositories.Competition
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly DapperContext _context;

        public PlayerRepository(DapperContext context) => _context = context;

        public async Task<bool> PublicSubmitPlayerAsync(Player phase)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Nickname", phase.Nickname);
            parameters.Add("@GameId", phase.GameId);
            parameters.Add("@RegistrationIp", phase.RegistrationIp);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_players_public_submit(@Nickname, @GameId, @RegistrationIp)",
                parameters,
                commandType: CommandType.Text
            );

            ValidateResponse(result);

            return true;
        }

        public async Task<IEnumerable<Player>> GetAllPlayersAsync(int pageNumber, int pageSize, string filter)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@FilterText", filter);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            return await connection.QueryAsync<Player>(
                "SELECT * FROM competition.fn_players_admin_list(@FilterText, @PageNumber, @PageSize)",
                parameters,
                commandType: CommandType.Text
            );
        }

        public async Task<bool> ProcessPlayerRequestAsync(int playerId, int newPlayerStateId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PlayerId", playerId);
            parameters.Add("@NewPlayerStateId", newPlayerStateId);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_players_process_request(@PlayerId, @NewPlayerStateId)",
                parameters,
                commandType: CommandType.Text
            );

            ValidateResponse(result);

            return true;
        }

        public async Task<bool> UpdatePlayerAsync(Player phase)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", phase.Id);
            parameters.Add("@Nickname", phase.Nickname);
            parameters.Add("@GameId", phase.GameId);
            parameters.Add("@StateId", phase.StateId);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_players_update(@Id, @Nickname, @GameId, @StateId)",
                parameters,
                commandType: CommandType.Text
            );

            ValidateResponse(result);

            return true;
        }

        public async Task<bool> SoftDeletePlayerAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_players_soft_delete(@Id)",
                new { Id = id },
                commandType: CommandType.Text
            );

            ValidateResponse(result);

            return true;
        }

        public async Task<IEnumerable<Player>> GetPlayersSelect()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Player>(
                "SELECT * FROM competition.fn_players_get_select()",
                commandType: CommandType.Text
            );
        }

        private static void ValidateResponse(ActionResult? result)
        {
            if (result is null || result.ErrorCode != 0)
            {
                throw new ApiException(result?.ErrorMessage ?? "Error desconocido.");
            }
        }
    }
}
