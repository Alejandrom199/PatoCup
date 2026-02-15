using PatoCup.Domain.Entities.Audit;

namespace PatoCup.Domain.Interfaces.Repositories.Audit
{
    public interface IAuditRepository
    {
        Task LogActionAsync(AuditLog audit);
        Task<IEnumerable<AuditLog>> GetAllLogsAsync(int pageNumber, int pageSize);
    }
}
