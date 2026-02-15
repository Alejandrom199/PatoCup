using Dapper;
using PatoCup.Domain.Entities.Audit;
using PatoCup.Domain.Interfaces.Repositories.Audit;
using System.Data;

namespace PatoCup.Infrastructure.Persistence.Repositories.Audit
{
    public class AuditRepository : IAuditRepository
    {
        private readonly DapperContext _context;

        public AuditRepository(DapperContext context) => _context = context;

        public async Task LogActionAsync(AuditLog audit)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", audit.UserId);
            parameters.Add("@ActionType", audit.ActionType);
            parameters.Add("@Message", audit.Message);
            parameters.Add("@IpAddress", audit.IpAddress);

            await connection.ExecuteAsync(
                "[Audit].[sp_SystemLogs_Create]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync(int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            return await connection.QueryAsync<AuditLog>(
                "[Audit].[sp_SystemLogs_GetAll]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
