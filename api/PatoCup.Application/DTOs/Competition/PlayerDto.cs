namespace PatoCup.Application.DTOs.Competition
{
    public class PublicSubmitPlayerDto
    {
        public string Nickname { get; set; } = null!;
        public string? GameId { get; set; } = null!;
        //public string? RegistrationIp { get; set; } = null!;
    }

    public class UpdatePlayerDto
    {
        public int Id { get; set; }
        public string Nickname { get; set; } = null!;
        public string? GameId { get; set; } = null!;
        public int StateId { get; set; }
    }

    public class PlayerResponseDto
    {
        public int Id { get; set; }
        public string? Nickname { get; set; } = string.Empty;
        public string? GameId { get; set; } = string.Empty;
        public string? RegistrationIp { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public int? PlayerStateId { get; set; }
        public string PlayerStateName { get; set; } = string.Empty;

        public int StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
    }

    public class PlayerSelectDto
    {
        public int Id { get; set; }
        public string? Nickname { get; set; } = string.Empty;
    }
}
