namespace SmartComplaint.Domain.Entities;

public class Attachment : BaseEntity
{
    public int AttachmentId { get; set; }
    public int ComplaintId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public Complaint Complaint { get; set; } = null!;
}