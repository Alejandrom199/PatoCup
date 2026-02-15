using System.ComponentModel.DataAnnotations;

namespace PatoCup.Application.DTOs.Competition
{
    public class CreateMatchDto
    {
        public int PhaseId { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
    }

    public class UpdateMatchDto
    {
        public int Id { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public int MatchStateId { get; set; }
        public int StateId { get; set; }
    }

    public class RegisterMatchResultDto
    {
        public int Id { get; set; }
        public int ScorePlayer1 { get; set; }
        public int ScorePlayer2 { get; set; }

        public int? WinnerId { get; set; } 
    }

    public class MatchResponseDto
    {
        public int Id { get; set; }

        public int PhaseId { get; set; }
        public string PhaseName { get; set; } = string.Empty;

        public int Player1Id { get; set; }
        public string Player1Name { get; set; } = string.Empty;

        public int Player2Id { get; set; }
        public string Player2Name { get; set; } = string.Empty;

        public int ScorePlayer1 { get; set; }
        public int ScorePlayer2 { get; set; }

        public int? WinnerId { get; set; }
        public string? WinnerName { get; set; } 

        public int MatchStateId { get; set; }
        public string MatchStateName { get; set; } = string.Empty;

        public int StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
    }
}