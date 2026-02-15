using Dapper;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Application.Exceptions;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Interfaces.Repositories.Competition;
using System.Data;

namespace PatoCup.Infrastructure.Persistence.Repositories.Competition
{
    public class TournamentRepository : ITournamentRepository
    {
        private readonly DapperContext _context;

        public TournamentRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<Tournament>> GetAllTournamentsAsync(int pageNumber, int pageSize, Tournament tournament)
        {
            using var connection = _context.CreateConnection();
            var NohaveTournament = tournament.TournamentStateId == 0;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            parameters.Add("@Name", tournament.Name);
            parameters.Add("@Description", tournament.Description);
            parameters.Add("@StartDate", tournament.StartDate == DateTime.MinValue ? null : tournament.StartDate);
            parameters.Add("@EndDate", tournament.EndDate == DateTime.MinValue ? null : tournament.EndDate);
            parameters.Add("@TournamentStateId", NohaveTournament ? null : tournament.TournamentStateId);

            return await connection.QueryAsync<Tournament>(
                "[Competition].[sp_Tournaments_GetAll]",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Tournament?> GetTournamentByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<Tournament>(
                "[Competition].[sp_Tournaments_GetById]",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }
        
        public async Task<int> CreateTournamentAsync(Tournament tournament)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Name", tournament.Name);
            parameters.Add("@Description", tournament.Description);
            parameters.Add("@StartDate", tournament.StartDate);
            parameters.Add("@EndDate", tournament.EndDate);

            AddOutputParameters(parameters);

            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("[Competition].[sp_Tournaments_Create]",
                parameters, commandType: CommandType.StoredProcedure);

            ValidateResponse(parameters);

            return parameters.Get<int>("NewId");
        }

        public async Task<bool> UpdateTournamentAsync(Tournament tournament)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", tournament.Id);
            parameters.Add("@Name", tournament.Name);
            parameters.Add("@Description", tournament.Description);
            parameters.Add("@StartDate", tournament.StartDate);
            parameters.Add("@EndDate", tournament.EndDate);
            parameters.Add("@TournamentStateId", tournament.TournamentStateId);
            parameters.Add("@StateId", tournament.StateId);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync("[Competition].[sp_Tournaments_Update]",
                parameters, commandType: CommandType.StoredProcedure);

            ValidateResponse(parameters);

            return true;
        }
        
        public async Task<bool> SoftDeleteTournamentAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            
            AddOutputParameters(parameters);

            await connection.ExecuteAsync("[Competition].[sp_Tournaments_SoftDelete]",
                parameters, commandType: CommandType.StoredProcedure);

            ValidateResponse(parameters);

            return true;
        }

        public async Task<bool> ReactivateTournamentAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            
            AddOutputParameters(parameters);

            await connection.ExecuteAsync("[Competition].[sp_Tournaments_Reactivate]",
                parameters, commandType: CommandType.StoredProcedure);

            ValidateResponse(parameters);

            return true;
        }

        public async Task<bool> SetPublicTournamentAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            AddOutputParameters(parameters);

            await connection.ExecuteAsync("[Competition].[sp_Tournaments_SetPublic]",
                parameters, commandType: CommandType.StoredProcedure);

            ValidateResponse(parameters);

            return true;
        }

        public async Task<Tournament?> GetPublicActiveTournamentAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<Tournament>(
                "[Competition].[sp_Tournaments_GetPublicActive]",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Tournament?> GetPublicBracketAsync()
        {
            using var connection = _context.CreateConnection();
            var tournamentDict = new Dictionary<int, Tournament>();
            var phaseDict = new Dictionary<int, Phase>();

            await connection.QueryAsync<Tournament, Phase, Match, Tournament>(
                "[Competition].[sp_Tournaments_GetPublicBracket]",
                (t, p, m) =>
                {
                    if (!tournamentDict.TryGetValue(t.Id, out var tournamentEntry))
                    {
                        tournamentEntry = t;
                        tournamentEntry.Phases = new List<Phase>();
                        tournamentDict.Add(t.Id, tournamentEntry);
                    }

                    if (p != null && p.Id > 0)
                    {
                        if (!phaseDict.TryGetValue(p.Id, out var phaseEntry))
                        {
                            phaseEntry = p;
                            phaseEntry.Matches = new List<Match>();
                            tournamentEntry.Phases.Add(phaseEntry);
                            phaseDict.Add(p.Id, phaseEntry);
                        }

                        if (m != null && m.Id > 0)
                        {
                            if (!phaseEntry.Matches.Any(x => x.Id == m.Id))
                            {
                                phaseEntry.Matches.Add(m);
                            }
                        }
                    }
                    return tournamentEntry;
                },
                splitOn: "Id,Id",
                commandType: CommandType.StoredProcedure
            );

            return tournamentDict.Values.FirstOrDefault();
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