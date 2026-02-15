namespace PatoCup.Application.DTOs.Competition
{
    public class CreatePhaseDto
    {
        public int TournamentId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UpdatePhaseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PhaseStateId { get; set; }
        public int StateId { get; set; }
        public int Sequence { get; set; }
    }

    public class PhaseResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = null!;

        public int PhaseStateId { get; set; }
        public string PhaseStateName { get; set; } = null!;

        public int StateId { get; set; }
        public string StateName { get; set; } = null!;
        public int Sequence { get; set; }
        public bool IsFinal { get; set; }

    }
}
