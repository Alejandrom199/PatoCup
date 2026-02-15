using PatoCup.Application.DTOs.Competition;
using PatoCup.Domain.Entities.Competition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Application.Interfaces.Services.Competition
{
    public interface IPlayerService
    {
        Task<bool> PublicSubmitPlayerAsync(PublicSubmitPlayerDto phase, string clientIp);
        Task<IEnumerable<PlayerResponseDto>> GetAllPlayersAsync(int pageNumber, int pageSize, string filter);
        Task<bool> ProcessPlayerRequestAsync(int playerId, int newPlayerStateId);
        Task<bool> UpdatePlayerAsync(UpdatePlayerDto phase);
        Task<bool> SoftDeletePlayerAsync(int id);
        Task<IEnumerable<PlayerSelectDto>> GetPlayersSelect();
    }
}
