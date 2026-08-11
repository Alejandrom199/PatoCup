using Dapper;
using PatoCup.Application.Exceptions;
using PatoCup.Domain.Entities.Security;
using PatoCup.Domain.Interfaces.Repositories.Security;
using System.Data;

namespace PatoCup.Infrastructure.Persistence.Repositories.Security
{
    public class UserRepository : IUserRepository
    {
        private readonly DapperContext _context;

        public UserRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(User user)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", user.RoleId);
            parameters.Add("@Username", user.Username);
            parameters.Add("@Password", user.Password);
            parameters.Add("@Email", user.Email);
            parameters.Add("@PhotoUrl", user.PhotoUrl);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM security.fn_users_create(@RoleId, @Username, @Password, @Email, @PhotoUrl)",
                parameters, commandType: CommandType.Text);

            ValidateResponse(result);

            return result!.NewId;
        }

        public async Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize, string? filterUsername, int? filterRoleId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@FilterUsername", filterUsername);
            parameters.Add("@FilterRoleId", filterRoleId);

            return await connection.QueryAsync<User>(
                "SELECT * FROM security.fn_users_get_all(@PageNumber, @PageSize, @FilterUsername, @FilterRoleId)",
                parameters,
                commandType: CommandType.Text);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM security.fn_users_get_by_id(@Id)",
                new { Id = id },
                commandType: CommandType.Text);
        }

        public async Task<bool> ReactivateAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM security.fn_users_reactivate(@Id)",
                new { Id = id }, commandType: CommandType.Text);

            ValidateResponse(result);

            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM security.fn_users_soft_delete(@Id)",
                new { Id = id }, commandType: CommandType.Text);

            ValidateResponse(result);

            return true;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", user.Id);
            parameters.Add("@RoleId", user.RoleId);
            parameters.Add("@Username", user.Username);
            parameters.Add("@Email", user.Email);
            parameters.Add("@PhotoUrl", user.PhotoUrl);
            parameters.Add("@StateId", user.StateId);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM security.fn_users_update(@Id, @RoleId, @Username, @Email, @PhotoUrl, @StateId)",
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
