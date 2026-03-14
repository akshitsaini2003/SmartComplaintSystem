namespace SmartComplaint.Domain.Entities;

public class ComplaintAssignment : BaseEntity
{
    public int AssignmentId { get; set; }
    public int ComplaintId { get; set; }
    public int AgentId { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public Complaint Complaint { get; set; } = null!;
    public User Agent { get; set; } = null!;
}