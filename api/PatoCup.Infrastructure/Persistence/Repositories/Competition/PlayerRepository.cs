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

            AddOutputParameters(parameters);

            await connection.ExecuteAsync(
                "[Competition].[sp_Players_PublicSubmit]",
                parameters, 
                commandType: CommandType.StoredProcedure
            );

            ValidateResponse(parameters);

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
                "[Competition].[sp_Players_AdminList]",
                parameters, 
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> ProcessPlayerRequestAsync(int playerId, int newPlayerStateId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PlayerId", playerId);
            parameters.Add("@NewPlayerStateId", newPlayerStateId);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync(
                "[Competition].[sp_Players_ProcessRequest]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            ValidateResponse(parameters);

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

            AddOutputParameters(parameters);

            await connection.ExecuteAsync(
                "[Competition].[sp_Players_Update]",
                parameters, 
                commandType: CommandType.StoredProcedure
            );

            ValidateResponse(parameters);

            return true;
        }

        public async Task<bool> SoftDeletePlayerAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync(
                "[Competition].[sp_Players_SoftDelete]",
                parameters, 
                commandType: CommandType.StoredProcedure
            );

            ValidateResponse(parameters);

            return true;
        }

        public async Task<IEnumerable<Player>> GetPlayersSelect()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Player>(
                "[Competition].[sp_Players_GetSelect]",
                commandType: CommandType.StoredProcedure
            );
        }

        private void AddOutputParameters(DynamicParameters parameters)
        {
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);
        }

        private void ValidateResponse(DynamicParameters parameters)
        {
            int errorCode = parameters.Get<int>("ErrorCode");
            string errorMessage = parameters.Get<string>("ErrorMessage");

            if (errorCode != 0)
            {
                throw new ApiException(errorMessage);
            }
        }
    }
}
