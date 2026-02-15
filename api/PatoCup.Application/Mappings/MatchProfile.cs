using AutoMapper;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Domain.Entities.Competition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Application.Mappings
{
    public class MatchProfile :  Profile
    {
        public MatchProfile()
        {
            // Mapeos de entrada
            CreateMap<CreateMatchDto, Match>();
            CreateMap<UpdateMatchDto, Match>();
            CreateMap<RegisterMatchResultDto, Match>();

            // Mapeo de salida
            CreateMap<Match, MatchResponseDto>();
        }
    }
}
