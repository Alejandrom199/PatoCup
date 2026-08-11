using Dapper;
using PatoCup.Domain.Entities.Common;
using PatoCup.Domain.Interfaces.Repositories.Common;
using System.Data;

namespace PatoCup.Infrastructure.Persistence.Repositories.Common
{
    public class CatalogRepository : ICatalogRepository
    {
        private readonly DapperContext _context;

        public CatalogRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<Catalog>> GeneralStatusGetAllAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Catalog>(
                "SELECT * FROM security.fn_general_status_get_all()",
                commandType: CommandType.Text
            );
        }

        public async Task<IEnumerable<Catalog>> PlayerStatusGetAllAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Catalog>(
                "SELECT * FROM competition.fn_player_status_get_all()",
                commandType: CommandType.Text
            );
        }

        public async Task<IEnumerable<Catalog>> TournamentStatusGetAllAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Catalog>(
                "SELECT * FROM competition.fn_tournament_status_get_all()",
                commandType: CommandType.Text
            );
        }

        public async Task<IEnumerable<Catalog>> PhaseStatusGetAllAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Catalog>(
                "SELECT * FROM competition.fn_phase_status_get_all()",
                commandType: CommandType.Text
            );
        }

        public async Task<IEnumerable<Catalog>> MatchStatusGetAllAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Catalog>(
                "SELECT * FROM competition.fn_match_status_get_all()",
                commandType: CommandType.Text
            );
        }

    }
}
