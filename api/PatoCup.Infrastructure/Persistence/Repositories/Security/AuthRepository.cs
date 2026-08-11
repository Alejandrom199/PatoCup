using Dapper;
using PatoCup.Application.Exceptions;
using PatoCup.Domain.Entities.Security;
using PatoCup.Domain.Interfaces.Repositories.Security;
using System.Data;

namespace PatoCup.Infrastructure.Persistence.Repositories.Security
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DapperContext _context;

        public AuthRepository(DapperContext context)
        {
            _context = context;
        }

        // fn_auth_login devuelve, en una sola fila, los datos del usuario Y el
        // error_code/error_message de validación (login inexistente / inactivo).
        // LoginResult extiende User para poder mapear ambos con una sola query.
        private sealed class LoginResult : User
        {
            public int ErrorCode { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
        }

        public async Task<User?> LoginAsync(string username)
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<LoginResult>(
                "SELECT * FROM security.fn_auth_login(@Username)",
                new { Username = username },
                commandType: CommandType.Text
            );

            if (result is null || result.ErrorCode != 0)
            {
                throw new ApiException(result?.ErrorMessage ?? "Error desconocido.");
            }

            return result;
        }

        public async Task<bool> ChangePassword(int id, string newPassword)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@NewPassword", newPassword);

            var result = await connection.QueryFirstOrDefaultAsync<ActionResult>(
                "SELECT * FROM security.fn_users_change_password(@Id, @NewPassword)",
                parameters, commandType: CommandType.Text);

            if (result is null || result.ErrorCode != 0)
            {
                throw new ApiException(result?.ErrorMessage ?? "Error desconocido.");
            }

            return true;
        }
    }
}
