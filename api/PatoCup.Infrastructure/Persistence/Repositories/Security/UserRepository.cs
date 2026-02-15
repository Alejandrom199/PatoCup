using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PatoCup.Application.Exceptions;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Entities.Security;
using PatoCup.Domain.Interfaces.Repositories.Security;
using System;
using System.Data;
using System.Threading.Tasks;

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

            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("[Security].[sp_Users_Create]", parameters, commandType: CommandType.StoredProcedure);

            int errorCode = parameters.Get<int>("@ErrorCode");
            string errorMessage = parameters.Get<string>("@ErrorMessage");

            if (errorCode != 0)
            {
                throw new Exception(errorMessage);
            }

            return parameters.Get<int>("@NewId");
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
                "[Security].[sp_Users_GetAll]", 
                parameters, 
                commandType: CommandType.StoredProcedure);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<User>(
                "[Competition].[sp_Users_GetById]",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> ReactivateAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("[Competition].[sp_Users_Reactivate]",
                parameters, commandType: CommandType.StoredProcedure);

            int errorCode = parameters.Get<int>("ErrorCode");
            string errorMessage = parameters.Get<string>("ErrorMessage");

            if (errorCode != 0)
            {
                throw new ApiException(errorMessage);
            }

            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("[Competition].[sp_Users_SoftDelete]",
                parameters, commandType: CommandType.StoredProcedure);

            int errorCode = parameters.Get<int>("ErrorCode");
            string errorMessage = parameters.Get<string>("ErrorMessage");

            if (errorCode != 0)
            {
                throw new ApiException(errorMessage);
            }

            return true;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", user.Id);
            parameters.Add("@RoleId", user.RoleId);
            parameters.Add("@Username", user.Username);
            parameters.Add("@Password", user.Password);
            parameters.Add("@Email", user.Email);
            parameters.Add("@PhotoUrl", user.PhotoUrl);
            parameters.Add("@StateId", user.StateId);

            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("[Security].[sp_Users_Update]", parameters, commandType: CommandType.StoredProcedure);

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