using AutoMapper;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Application.Interfaces.Services.Competition;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Interfaces.Repositories.Competition;

namespace PatoCup.Application.UseCases.Competition
{
    public class PlayerService :  IPlayerService
    {
        private readonly IPlayerRepository _repository;
        private readonly IMapper _mapper;

        public PlayerService(IPlayerRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PlayerResponseDto>> GetAllPlayers(int pageNumber, int pageSize, string filter)
        {
            var entities = await _repository.GetAllPlayersAsync(pageNumber, pageSize, filter);

            var mappedEntities = _mapper.Map<IEnumerable<PlayerResponseDto>>(entities);
            
            return mappedEntities;
        }

        public async Task<IEnumerable<PlayerSelectDto>> GetPlayersSelect()
        {
            var entities = await _repository.GetPlayersSelect();

            var mappedEntities = _mapper.Map<IEnumerable<PlayerSelectDto>>(entities);

            return mappedEntities;
        }

        public async Task<bool> ProcessPlayerRequest(int playerId, int newPlayerStateId) => await _repository.ProcessPlayerRequestAsync(playerId, newPlayerStateId);

        public async Task<bool> PublicSubmitPlayer(PublicSubmitPlayerDto phase, string clientIp)
        {
            var entity = _mapper.Map<Player>(phase);

            entity.RegistrationIp = clientIp;

            return await _repository.PublicSubmitPlayerAsync(entity);
        }

        public async Task<bool> UpdatePlayer(UpdatePlayerDto phase)
        {
            var entity = _mapper.Map<Player>(phase);
            return await _repository.UpdatePlayerAsync(entity);
        }

        public async Task<bool> SoftDeletePlayer(int id) => await _repository.SoftDeletePlayerAsync(id);

    }
}
