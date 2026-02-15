using PatoCup.Application.DTOs.Competition;

namespace PatoCup.Application.Interfaces.Services.Competition
{
    public interface IMatchService
    {
        Task<int> CreateMatch(CreateMatchDto dto);
        Task<bool> UpdateMatch(UpdateMatchDto dto);
        Task<bool> RegisterResult(RegisterMatchResultDto dto);
        Task<IEnumerable<MatchResponseDto>> GetMatchesByPhaseId(int phaseId);
        Task<bool> SoftDeleteMatch(int id);
    }
}