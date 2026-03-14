using SmartComplaint.Domain.Entities;

namespace SmartComplaint.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Category> Categories { get; }
    IRepository<Complaint> Complaints { get; }
    IRepository<ComplaintAssignment> ComplaintAssignments { get; }
    IRepository<Comment> Comments { get; }
    IRepository<Attachment> Attachments { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<ComplaintHistory> ComplaintHistories { get; }
    IRepository<SLAPolicy> SLAPolicies { get; }
    IRepository<Feedback> Feedbacks { get; }
    IRepository<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync();
}