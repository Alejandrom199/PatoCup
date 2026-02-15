using AutoMapper;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Domain.Entities.Competition;

namespace PatoCup.Application.Mappings
{
    public class PhaseProfile : Profile
    {
        public PhaseProfile()
        {
            // Mapeos de entrada
            CreateMap<CreatePhaseDto, Phase>();
            CreateMap<UpdatePhaseDto, Phase>();

            // Mapeo de salida
            CreateMap<Phase, PhaseResponseDto>();
        }
    }
}
