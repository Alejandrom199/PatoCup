using PatoCup.Application.DTOs.Audit;

namespace PatoCup.Application.Interfaces.Services.Audit
{
    public interface IAuditService
    {
        Task LogAction(CreateAuditLogDto audit);
        Task<IEnumerable<AuditLogResponseDto>> GetAllLogs(int pageNumber, int pageSize);
    }
}
