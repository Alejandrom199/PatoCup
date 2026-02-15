using PatoCup.Domain.Entities.Competition;

namespace PatoCup.Application.DTOs.Competition
{
    public class CreateTournamentDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Phase> Phases { get; set; } = new();
    }

    public class UpdateTournamentDto : CreateTournamentDto
    {
        public int Id { get; set; }
        public int TournamentStateId { get; set; }
        public int StateId { get; set; }
    }

    public class TournamentResponseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TournamentStateId { get; set; }
        public string TournamentStateName { get; set; } = null!;
        public int StateId { get; set; }
        public string StateName { get; set; } = null!;
        public bool IsPublic { get; set; }
    }

    public class TournamentBracketDto
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = null!;
        public List<PhaseBracketDto> Phases { get; set; } = new();
    }

    public class PhaseBracketDto
    {
        public int PhaseId { get; set; }
        public string PhaseName { get; set; } = null!;
        public int PhaseOrder { get; set; }
        public bool IsFinal { get; set; }
        public List<MatchBracketDto> Matches { get; set; } = new();
    }

    public class MatchBracketDto
    {
        public int MatchId { get; set; }
        public string Player1Name { get; set; }
        public string Player2Name { get; set; } 
        public int? ScorePlayer1 { get; set; }
        public int? ScorePlayer2 { get; set; }
    }

}