namespace SmartComplaint.Domain.Entities;

public class Comment : BaseEntity
{
    public int CommentId { get; set; }
    public int ComplaintId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;

    // Navigation
    public Complaint Complaint { get; set; } = null!;
    public User User { get; set; } = null!;
}