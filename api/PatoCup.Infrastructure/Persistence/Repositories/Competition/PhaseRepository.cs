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

            var parameters = new DynamicParameters();

            return await connection.QueryAsync<Phase>(
                "[Competition].[sp_Phases_GetAll]",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Phase?> GetPhaseByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<Phase>(
                "[Competition].[sp_Phases_GetById]",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreatePhaseAsync(Phase phase)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("TournamentId", phase.TournamentId);
            parameters.Add("Name", phase.Name);

            AddOutputParameters(parameters);

            parameters.Add("NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("[Competition].[sp_Phases_Create]",
                parameters, commandType: CommandType.StoredProcedure);

            ValidateResponse(parameters);

            return parameters.Get<int>("NewId");
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

            AddOutputParameters(parameters);

            await connection.ExecuteAsync("[Competition].[sp_Phases_Update]",
                parameters, commandType: CommandType.StoredProcedure);

            ValidateResponse(parameters);

            return true;
        }

        public async Task<bool> SoftDeletePhaseAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync("[Competition].[sp_Phases_SoftDelete]",
                parameters, commandType: CommandType.StoredProcedure);

            ValidateResponse(parameters);

            return true;
        }

        public async Task<bool> ReactivatePhaseAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync("[Competition].[sp_Phases_Reactivate]",
                parameters, commandType: CommandType.StoredProcedure);

            ValidateResponse(parameters);

            return true;
        }

        public async Task<IEnumerable<Phase>> GetPhasesByTournamentIdAsync(int tournamentId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TournamentId", tournamentId);

            var entities = await connection.QueryAsync<Phase>(
                "[Competition].[sp_Phases_GetByTournamentId]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return entities;
        }

        public async Task<bool> SetFinalPhaseAsync(int tournamentId, int phaseId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@TournamentId", tournamentId);
            parameters.Add("@PhaseId", phaseId);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync("[Competition].[sp_Phases_SetFinal]",
                parameters, commandType: CommandType.StoredProcedure);

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
