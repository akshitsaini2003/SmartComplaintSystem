namespace SmartComplaint.Application.DTOs;

public class NotificationResponseDto
{
    public int NotificationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
}