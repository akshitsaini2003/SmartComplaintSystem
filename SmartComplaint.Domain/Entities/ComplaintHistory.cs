using SmartComplaint.Domain.Enums;

namespace SmartComplaint.Domain.Entities;

public class ComplaintHistory
{
    public int HistoryId { get; set; }
    public int ComplaintId { get; set; }
    public ComplaintStatus OldStatus { get; set; }
    public ComplaintStatus NewStatus { get; set; }
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
    public string ChangedBy { get; set; } = string.Empty;

    // Navigation
    public Complaint Complaint { get; set; } = null!;
}