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

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_matches_create(@PhaseId, @Player1Id, @Player2Id)",
                parameters,
                commandType: CommandType.Text
            );

            ValidateResponse(result);

            return result!.NewId;
        }

        public async Task<bool> UpdateMatchAsync(Match match)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", match.Id);
            parameters.Add("@Player1Id", match.Player1Id);
            parameters.Add("@Player2Id", match.Player2Id);
            parameters.Add("@MatchStateId", match.MatchStateId);
            parameters.Add("@StateId", match.StateId);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_matches_update(@Id, @Player1Id, @Player2Id, @MatchStateId, @StateId)",
                parameters,
                commandType: CommandType.Text
            );

            ValidateResponse(result);

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

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_matches_register_result(@Id, @ScorePlayer1, @ScorePlayer2, @WinnerId)",
                parameters,
                commandType: CommandType.Text
            );

            ValidateResponse(result);

            return true;
        }

        public async Task<IEnumerable<Match>> GetMatchesByPhaseIdAsync(int phaseId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();

            parameters.Add("@PhaseId", phaseId);

            return await connection.QueryAsync<Match>(
                "SELECT * FROM competition.fn_matches_get_by_phase_id(@PhaseId)",
                parameters,
                commandType: CommandType.Text
            );
        }

        public async Task<bool> SoftDeleteMatchAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_matches_soft_delete(@Id)",
                new { Id = id },
                commandType: CommandType.Text
            );

            ValidateResponse(result);

            return true;
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
