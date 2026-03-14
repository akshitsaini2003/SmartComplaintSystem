namespace SmartComplaint.Domain.Entities;

public class Notification : BaseEntity
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;

    // Navigation
    public User User { get; set; } = null!;
}