using AutoMapper;
using PatoCup.Application.DTOs.Audit;
using PatoCup.Application.Interfaces.Services.Audit;
using PatoCup.Domain.Entities.Audit;
using PatoCup.Domain.Interfaces.Repositories.Audit;

namespace PatoCup.Application.UseCases.Audit
{
    public class AuditService : IAuditService
    {

        private readonly IAuditRepository _repository;
        private readonly IMapper _mapper;

        public AuditService(IAuditRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetAllLogs(int pageNumber, int pageSize)
        {
            var entities = await _repository.GetAllLogsAsync(pageNumber, pageSize);

            var mappedEntities = _mapper.Map<IEnumerable<AuditLogResponseDto>>(entities);

            return mappedEntities;
        }

        public async Task LogAction(CreateAuditLogDto audit)
        {
            var entity = _mapper.Map<AuditLog>(audit);

            await _repository.LogActionAsync(entity);
        }
    }
}
