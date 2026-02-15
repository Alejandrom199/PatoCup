using AutoMapper;
using PatoCup.Application.DTOs.Security;
using PatoCup.Domain.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Application.Mappings
{
    public class MenuProfile : Profile
    {
        public MenuProfile()
        {
            // Mapeos de salida
            CreateMap<Menu, MenuResponseDto>();
        }
    }
}
