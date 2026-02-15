using AutoMapper;
using PatoCup.Application.DTOs.Common;
using PatoCup.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Application.Mappings
{
    public class CatalogProfile :  Profile
    {
        public CatalogProfile()
        {
            CreateMap<CatalogDto, Catalog>();
            CreateMap<Catalog, CatalogDto>();
        }
    }
}
