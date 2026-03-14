namespace SmartComplaint.Domain.Entities;

public class AuditLog
{
    public int AuditId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}