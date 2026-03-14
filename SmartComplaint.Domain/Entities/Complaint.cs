using SmartComplaint.Domain.Enums;
using System.Net.Mail;
using System.Xml.Linq;

namespace SmartComplaint.Domain.Entities;

public class Complaint : BaseEntity
{
    public int ComplaintId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Priority Priority { get; set; } = Priority.Low;
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;
    public int UserId { get; set; }
    public int CategoryId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<ComplaintHistory> Histories { get; set; } = new List<ComplaintHistory>();
    public ICollection<ComplaintAssignment> Assignments { get; set; } = new List<ComplaintAssignment>();
    public Feedback? Feedback { get; set; }
}