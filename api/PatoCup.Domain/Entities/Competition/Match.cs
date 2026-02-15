using PatoCup.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Domain.Entities.Competition
{
    public class Match : BaseEntity
    {
        public int PhaseId { get; set; }
        public string PhaseName { get; set; } = string.Empty;

        public int Player1Id { get; set; }
        public string Player1Name { get; set; } = string.Empty;

        public int Player2Id { get; set; }
        public string Player2Name { get; set; } = string.Empty;

        public int ScorePlayer1 { get; set; }
        public int ScorePlayer2 { get; set; }

        public int? WinnerId { get; set; }
        public string? WinnerName { get; set; } = string.Empty;

        public int MatchStateId { get; set; }
        public string MatchStateName { get; set; } = string.Empty;

        public int StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
    }
}
