using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatoCup.Application.DTOs.Audit
{
    public class CreateAuditLogDto
    {
        public int? UserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? IPAddress { get; set; } = string.Empty;
    }

    public class AuditLogResponseDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? IPAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalRecords { get; set; }
    }
}
