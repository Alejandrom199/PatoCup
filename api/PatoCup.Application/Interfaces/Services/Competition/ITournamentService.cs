using PatoCup.Application.DTOs.Common;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Domain.Entities.Competition;

namespace PatoCup.Application.Interfaces.Services.Competition
{
    public interface ITournamentService
    {
        Task<IEnumerable<TournamentResponseDto>> GetAllTournaments(TournamentQueryDto query);
        Task<TournamentResponseDto?> GetTournamentById(int id);
        Task<int> CreateTournament(CreateTournamentDto tournamentDto);
        Task<bool> UpdateTournament(UpdateTournamentDto tournamentDto);
        Task<bool> SoftDeleteTournament(int id);
        Task<bool> ReactivateTournament(int id);
        Task<bool> SetPublicTournament(int id);
        Task<TournamentResponseDto?> GetPublicActiveTournament();
        Task<TournamentBracketDto?> GetPublicBracket();
    }
}