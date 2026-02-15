using PatoCup.Domain.Common;

namespace PatoCup.Domain.Entities.Security
{
    public class User : BaseEntity
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;

        public int StateId { get; set; }
        public string? StateName { get; set; }
    }
}