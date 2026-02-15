using Dapper;
using PatoCup.Application.Exceptions;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Interfaces.Repositories.Competition;
using System.Data;

namespace PatoCup.Infrastructure.Persistence.Repositories.Competition
{
    public class MatchRepository : IMatchRepository
    {
        private readonly DapperContext _context;

        public MatchRepository(DapperContext context) => _context = context;

        public async Task<int> CreateMatchAsync(Match match)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();

            parameters.Add("@PhaseId", match.PhaseId);
            parameters.Add("@Player1Id", match.Player1Id);
            parameters.Add("@Player2Id", match.Player2Id);

            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync(
                "[Competition].[sp_Matches_Create]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            ValidateResponse(parameters);

            return parameters.Get<int>("@NewId");
        }

        public async Task<bool> UpdateMatchAsync(Match match)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", match.Id);
            parameters.Add("@Player1Id", match.Player1Id);
            parameters.Add("@Player2Id", match.Player2Id);
            parameters.Add("@MatchStateId", match.MatchStateId);
            parameters.Add("StateId", match.StateId);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync(
                "[Competition].[sp_Matches_Update]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            ValidateResponse(parameters);

            return true;
        }

        public async Task<bool> RegisterResultAsync(Match match)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();

            parameters.Add("@Id", match.Id);
            parameters.Add("@ScorePlayer1", match.ScorePlayer1);
            parameters.Add("@ScorePlayer2", match.ScorePlayer2);
            parameters.Add("@WinnerId", match.WinnerId);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync(
                "[Competition].[sp_Matches_RegisterResult]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            ValidateResponse(parameters);

            return true;
        }

        public async Task<IEnumerable<Match>> GetMatchesByPhaseIdAsync(int phaseId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();

            parameters.Add("@PhaseId", phaseId);

            return await connection.QueryAsync<Match>(
                "[Competition].[sp_Matches_GetByPhaseId]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> SoftDeleteMatchAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();

            parameters.Add("@Id", id);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync(
                "[Competition].[sp_Matches_SoftDelete]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            ValidateResponse(parameters);

            return true;
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