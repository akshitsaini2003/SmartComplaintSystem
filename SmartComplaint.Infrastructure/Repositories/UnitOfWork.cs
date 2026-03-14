using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Infrastructure.Data;

namespace SmartComplaint.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IRepository<User> Users { get; }
    public IRepository<Category> Categories { get; }
    public IRepository<Complaint> Complaints { get; }
    public IRepository<ComplaintAssignment> ComplaintAssignments { get; }
    public IRepository<Comment> Comments { get; } 
    public IRepository<Attachment> Attachments { get; }
    public IRepository<Notification> Notifications { get; }
    public IRepository<ComplaintHistory> ComplaintHistories { get; }
    public IRepository<SLAPolicy> SLAPolicies { get; }
    public IRepository<Feedback> Feedbacks { get; }
    public IRepository<AuditLog> AuditLogs { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new Repository<User>(context);
        Categories = new Repository<Category>(context);
        Complaints = new Repository<Complaint>(context);
        ComplaintAssignments = new Repository<ComplaintAssignment>(context);
        Comments = new Repository<Comment>(context);
        Attachments = new Repository<Attachment>(context);
        Notifications = new Repository<Notification>(context);
        ComplaintHistories = new Repository<ComplaintHistory>(context);
        SLAPolicies = new Repository<SLAPolicy>(context);
        Feedbacks = new Repository<Feedback>(context);
        AuditLogs = new Repository<AuditLog>(context);
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}