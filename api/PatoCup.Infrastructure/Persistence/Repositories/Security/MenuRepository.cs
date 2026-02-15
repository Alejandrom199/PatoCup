using Dapper;
using PatoCup.Domain.Entities.Security;
using PatoCup.Domain.Interfaces.Repositories.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Infrastructure.Persistence.Repositories.Security
{
    public class MenuRepository : IMenuRepository
    {
        private readonly DapperContext _context;

        public MenuRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Menu>> GetMenuByUserAsync(int userId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await connection.QueryAsync<Menu>(
                "[Security].[sp_Security_GetMenuByUserId]",
                parameters,
                commandType: CommandType.StoredProcedure);
        }


    }
}
