namespace SmartComplaint.Domain.Entities;

public class Feedback : BaseEntity
{
    public int FeedbackId { get; set; }
    public int ComplaintId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }

    // Navigation
    public Complaint Complaint { get; set; } = null!;
    public User User { get; set; } = null!;
}