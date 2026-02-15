using PatoCup.Domain.Common;

namespace PatoCup.Domain.Entities.Competition
{
    public class Tournament : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TournamentStateId { get; set; }
        public string? TournamentStateName { get; set; }
        public int StateId { get; set; }
        public string? StateName { get; set; }
        public bool? IsPublic { get; set; }
        public List<Phase> Phases { get; set; } = new();
    }
}