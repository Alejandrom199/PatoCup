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
        Task<bool> PublicSubmitPlayer(PublicSubmitPlayerDto phase, string clientIp);
        Task<IEnumerable<PlayerResponseDto>> GetAllPlayers(int pageNumber, int pageSize, string filter);
        Task<bool> ProcessPlayerRequest(int playerId, int newPlayerStateId);
        Task<bool> UpdatePlayer(UpdatePlayerDto phase);
        Task<bool> SoftDeletePlayer(int id);
        Task<IEnumerable<PlayerSelectDto>> GetPlayersSelect();
    }
}
