using PatoCup.Domain.Common;

namespace PatoCup.Domain.Entities.Competition
{
    public class Player :  BaseEntity
    {
        public string Nickname { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        public string RegistrationIp { get; set; } = string.Empty;
        
        public int PlayerStateId { get; set; }
        public string PlayerStateName { get; set; } = string.Empty;

        public int StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
    }
}
