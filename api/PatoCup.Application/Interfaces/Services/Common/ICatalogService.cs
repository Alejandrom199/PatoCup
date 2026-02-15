using PatoCup.Application.DTOs.Common;
using PatoCup.Application.DTOs.Competition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Application.Interfaces.Services.Common
{
    public interface ICatalogService
    {
        Task<IEnumerable<CatalogDto>> GeneralStatusGetAll();
        Task<IEnumerable<CatalogDto>> PlayerStatusGetAll();
        Task<IEnumerable<CatalogDto>> TournamentStatusGetAll();
        Task<IEnumerable<CatalogDto>> PhaseStatusGetAll();
        Task<IEnumerable<CatalogDto>> MatchStatusGetAll();
    }
}
