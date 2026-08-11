using Dapper;
using PatoCup.Domain.Entities.Security;
using PatoCup.Domain.Interfaces.Repositories.Security;
using System.Data;

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
                "SELECT * FROM security.fn_security_get_menu_by_user_id(@UserId)",
                parameters,
                commandType: CommandType.Text);
        }
    }
}
