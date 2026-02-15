namespace PatoCup.Domain.Entities.Audit
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string ActionType { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalRecords { get; set; }
    }

}