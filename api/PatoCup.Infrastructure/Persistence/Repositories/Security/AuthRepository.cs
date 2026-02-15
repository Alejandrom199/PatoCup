
using Dapper;
using Microsoft.Data.SqlClient;
using PatoCup.Domain.Entities.Security;
using System.Data;
using System.Threading.Tasks;
using System;
using PatoCup.Domain.Interfaces.Repositories.Security;

namespace PatoCup.Infrastructure.Persistence.Repositories.Security
    {
        public class AuthRepository : IAuthRepository
        {
            private readonly DapperContext _context;

            public AuthRepository(DapperContext context)
            {
                _context = context;
            }

        public async Task<User?> LoginAsync(string username)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);

            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "[Security].[sp_Auth_Login]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            int errorCode = parameters.Get<int>("@ErrorCode");
            string errorMessage = parameters.Get<string>("@ErrorMessage");

            if (errorCode != 0)
            {
                throw new Exception(errorMessage);
            }

            return user;
        }

        public async Task<bool> ChangePassword(int id, string newPassword)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@NewPassword", newPassword);

            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("[Security].[sp_Users_ChangePassword]", parameters, commandType: CommandType.StoredProcedure);

            int errorCode = parameters.Get<int>("@ErrorCode");
            string errorMessage = parameters.Get<string>("@ErrorMessage");

            if (errorCode != 0)
            {
                throw new Exception(errorMessage);
            }

            return true;
        }
    }
}