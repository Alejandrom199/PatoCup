using PatoCup.Domain.Common;

namespace PatoCup.Domain.Entities.Competition
{
    public class Phase : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public int PhaseStateId { get; set; }
        public string PhaseStateName { get; set; } = string.Empty;
        public int StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public bool IsFinal { get; set; }
        public List<Match> Matches { get; set; } = new();
    }
}
