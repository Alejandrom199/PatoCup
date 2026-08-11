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
                "SELECT * FROM competition.fn_tournaments_get_all(@PageNumber, @PageSize, @Name, @Description, @StartDate, @EndDate, @TournamentStateId)",
                parameters,
                commandType: CommandType.Text);
        }

        public async Task<Tournament?> GetTournamentByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<Tournament>(
                "SELECT * FROM competition.fn_tournaments_get_by_id(@Id)",
                new { Id = id },
                commandType: CommandType.Text);
        }

        public async Task<int> CreateTournamentAsync(Tournament tournament)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Name", tournament.Name);
            parameters.Add("@Description", tournament.Description);
            parameters.Add("@StartDate", tournament.StartDate);
            parameters.Add("@EndDate", tournament.EndDate);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_tournaments_create(@Name, @Description, @StartDate, @EndDate)",
                parameters, commandType: CommandType.Text);

            ValidateResponse(result);

            return result!.NewId;
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

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_tournaments_update(@Id, @Name, @Description, @StartDate, @EndDate, @TournamentStateId, @StateId)",
                parameters, commandType: CommandType.Text);

            ValidateResponse(result);

            return true;
        }

        public async Task<bool> SoftDeleteTournamentAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_tournaments_soft_delete(@Id)",
                new { Id = id }, commandType: CommandType.Text);

            ValidateResponse(result);

            return true;
        }

        public async Task<bool> ReactivateTournamentAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_tournaments_reactivate(@Id)",
                new { Id = id }, commandType: CommandType.Text);

            ValidateResponse(result);

            return true;
        }

        public async Task<bool> SetPublicTournamentAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM competition.fn_tournaments_set_public(@Id)",
                new { Id = id }, commandType: CommandType.Text);

            ValidateResponse(result);

            return true;
        }

        public async Task<Tournament?> GetPublicActiveTournamentAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<Tournament>(
                "SELECT * FROM competition.fn_tournaments_get_public_active()",
                commandType: CommandType.Text);
        }

        public async Task<Tournament?> GetPublicBracketAsync()
        {
            using var connection = _context.CreateConnection();
            var tournamentDict = new Dictionary<int, Tournament>();
            var phaseDict = new Dictionary<int, Phase>();

            // Postgres exige nombres de columna únicos en RETURNS TABLE, así que la función
            // devuelve columnas con prefijo (tournament_id, phase_id, match_id...). Acá se
            // re-alias a "Id"/"Name" repetidos para reconstruir el mismo truco de splitOn
            // que usaba el SP de SQL Server.
            const string sql = @"
                SELECT
                    tournament_id AS ""Id"", tournament_name AS ""Name"",
                    phase_id AS ""Id"", phase_name AS ""Name"", sequence AS ""Sequence"", is_final AS ""IsFinal"",
                    match_id AS ""Id"", player1_name AS ""Player1Name"", player2_name AS ""Player2Name"",
                    score_player1 AS ""ScorePlayer1"", score_player2 AS ""ScorePlayer2"", match_phase_name AS ""PhaseName""
                FROM competition.fn_tournaments_get_public_bracket()";

            await connection.QueryAsync<Tournament, Phase, Match, Tournament>(
                sql,
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
                commandType: CommandType.Text
            );

            return tournamentDict.Values.FirstOrDefault();
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
