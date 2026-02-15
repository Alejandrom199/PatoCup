using AutoMapper;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Domain.Entities.Competition;

namespace PatoCup.Application.Mappings
{
    public class TournamentProfile : Profile
    {
        public TournamentProfile()
        {
            // Mapeos de entrada
            CreateMap<TournamentQueryDto, Tournament>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? string.Empty))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate ?? DateTime.MinValue))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate ?? DateTime.MinValue))
                .ForMember(dest => dest.TournamentStateId, opt => opt.MapFrom(src => src.TournamentStateId ?? 0));

            CreateMap<CreateTournamentDto, Tournament>();

            CreateMap<UpdateTournamentDto, Tournament>();

            // Mapeo de salida
            CreateMap<Tournament, TournamentResponseDto>();

            // Mapeo de Match
            CreateMap<Match, MatchBracketDto>()
                .ForMember(dest => dest.MatchId, opt => opt.MapFrom(src => src.Id));

            // Mapeo de Phase
            CreateMap<Phase, PhaseBracketDto>()
                .ForMember(dest => dest.PhaseId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PhaseName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PhaseOrder, opt => opt.MapFrom(src => src.Sequence));

            // Mapeo de Tournament
            CreateMap<Tournament, TournamentBracketDto>()
                .ForMember(dest => dest.TournamentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TournamentName, opt => opt.MapFrom(src => src.Name));
        }
    }
}