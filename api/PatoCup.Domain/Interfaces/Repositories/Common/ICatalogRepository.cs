using PatoCup.Domain.Entities.Common;
using PatoCup.Domain.Entities.Competition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Domain.Interfaces.Repositories.Common
{
    public interface ICatalogRepository
    {
        Task<IEnumerable<Catalog>> GeneralStatusGetAllAsync();
        Task<IEnumerable<Catalog>> PlayerStatusGetAllAsync();
        Task<IEnumerable<Catalog>> TournamentStatusGetAllAsync();
        Task<IEnumerable<Catalog>> PhaseStatusGetAllAsync();
        Task<IEnumerable<Catalog>> MatchStatusGetAllAsync();
    }
}
