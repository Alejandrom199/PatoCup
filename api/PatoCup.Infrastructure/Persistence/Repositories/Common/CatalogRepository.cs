using Dapper;
using PatoCup.Domain.Entities.Common;
using PatoCup.Domain.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                "[Security].[sp_GeneralStatus_GetAll]",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<Catalog>> PlayerStatusGetAllAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Catalog>(
                "[Competition].[sp_PlayerStatus_GetAll]",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<Catalog>> TournamentStatusGetAllAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Catalog>(
                "[Competition].[sp_TournamentStatus_GetAll]",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<Catalog>> PhaseStatusGetAllAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Catalog>(
                "[Competition].[sp_PhaseStatus_GetAll]",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<Catalog>> MatchStatusGetAllAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<Catalog>(
                "[Competition].[sp_MatchStatus_GetAll]",
                commandType: CommandType.StoredProcedure
            );
        }

    }
}
