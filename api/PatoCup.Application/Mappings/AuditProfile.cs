using AutoMapper;
using PatoCup.Application.DTOs.Audit;
using PatoCup.Domain.Entities.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Application.Mappings
{
    public class AuditProfile : Profile
    {
        public AuditProfile()
        {
            // Mapeos de entrada
            CreateMap<CreateAuditLogDto, AuditLog>();

            // Mapeos de salida
            CreateMap<AuditLog, AuditLogResponseDto>();
        }
    }
}
