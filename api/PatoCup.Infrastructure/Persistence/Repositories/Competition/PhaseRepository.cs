using Dapper;
using PatoCup.Application.Exceptions;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Interfaces.Repositories.Competition;
using System.Data;

namespace PatoCup.Infrastructure.Persistence.Repositories.Competition
{
    public class PhaseRepository : IPhaseRepository
    {
        private readonly DapperContext _context;

        public PhaseRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<Phase>> GetAllPhasesAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Phase>(
                "SELECT * FROM competition.fn_phases_get_all()",
                commandType: CommandType.Text);
        }

        public async Task<Phase?> GetPhaseByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<Phase>(
                "SELECT * FROM competition.fn_phases_get_by_id(@Id)",
                new { Id = id },
                commandType: CommandType.Text);
        }

        public async Task<int> CreatePhaseAsync(Phase phase)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TournamentId", phase.TournamentId);
            parameters.Add("@Name", phase.Name);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_phases_create(@TournamentId, @Name)",
                parameters, commandType: CommandType.Text);

            ValidateResponse(result);

            return result!.NewId;
        }

        public async Task<bool> UpdatePhaseAsync(Phase phase)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", phase.Id);
            parameters.Add("@Name", phase.Name);
            parameters.Add("@PhaseStateId", phase.PhaseStateId);
            parameters.Add("@StateId", phase.StateId);
            parameters.Add("@Sequence", phase.Sequence);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_phases_update(@Id, @Name, @PhaseStateId, @StateId, @Sequence)",
                parameters, commandType: CommandType.Text);

            ValidateResponse(result);

            return true;
        }

        public async Task<bool> SoftDeletePhaseAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_phases_soft_delete(@Id)",
                new { Id = id }, commandType: CommandType.Text);

            ValidateResponse(result);

            return true;
        }

        public async Task<bool> ReactivatePhaseAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_phases_reactivate(@Id)",
                new { Id = id }, commandType: CommandType.Text);

            ValidateResponse(result);

            return true;
        }

        public async Task<IEnumerable<Phase>> GetPhasesByTournamentIdAsync(int tournamentId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TournamentId", tournamentId);

            var entities = await connection.QueryAsync<Phase>(
                "SELECT * FROM competition.fn_phases_get_by_tournament_id(@TournamentId)",
                parameters,
                commandType: CommandType.Text
            );

            return entities;
        }

        public async Task<bool> SetFinalPhaseAsync(int tournamentId, int phaseId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@TournamentId", tournamentId);
            parameters.Add("@PhaseId", phaseId);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_phases_set_final(@TournamentId, @PhaseId)",
                parameters, commandType: CommandType.Text);

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
