using Microsoft.EntityFrameworkCore;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Domain.Enums;

namespace SmartComplaint.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<ComplaintAssignment> ComplaintAssignments => Set<ComplaintAssignment>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ComplaintHistory> ComplaintHistories => Set<ComplaintHistory>();
    public DbSet<SLAPolicy> SLAPolicies => Set<SLAPolicy>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── User ───────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role)
                  .HasConversion<string>();
        });

        // ─── Category ────────────────────────────────────────
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        // ─── Complaint ───────────────────────────────────────
        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(c => c.ComplaintId);
            entity.Property(c => c.Title).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).IsRequired();
            entity.Property(c => c.Priority).HasConversion<string>();
            entity.Property(c => c.Status).HasConversion<string>();

            entity.HasOne(c => c.User)
                  .WithMany(u => u.Complaints)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Category)
                  .WithMany(cat => cat.Complaints)
                  .HasForeignKey(c => c.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── ComplaintAssignment ─────────────────────────────
        modelBuilder.Entity<ComplaintAssignment>(entity =>
        {
            entity.HasKey(a => a.AssignmentId);

            entity.HasOne(a => a.Complaint)
                  .WithMany(c => c.Assignments)
                  .HasForeignKey(a => a.ComplaintId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Agent)
                  .WithMany()
                  .HasForeignKey(a => a.AgentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Comment ─────────────────────────────────────────
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.CommentId);

            entity.HasOne(c => c.Complaint)
                  .WithMany(c => c.Comments)
                  .HasForeignKey(c => c.ComplaintId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Attachment ──────────────────────────────────────
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(a => a.AttachmentId);

            entity.HasOne(a => a.Complaint)
                  .WithMany(c => c.Attachments)
                  .HasForeignKey(a => a.ComplaintId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Notification ────────────────────────────────────
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.NotificationId);

            entity.HasOne(n => n.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── ComplaintHistory ────────────────────────────────
        modelBuilder.Entity<ComplaintHistory>(entity =>
        {
            entity.HasKey(h => h.HistoryId);
            entity.Property(h => h.OldStatus).HasConversion<string>();
            entity.Property(h => h.NewStatus).HasConversion<string>();

            entity.HasOne(h => h.Complaint)
                  .WithMany(c => c.Histories)
                  .HasForeignKey(h => h.ComplaintId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── SLAPolicy ───────────────────────────────────────
        modelBuilder.Entity<SLAPolicy>(entity =>
        {
            entity.HasKey(s => s.SLAPolicyId);
            entity.Property(s => s.Priority).HasConversion<string>();
        });

        // ─── Feedback ────────────────────────────────────────
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(f => f.FeedbackId);

            entity.HasOne(f => f.Complaint)
                  .WithOne(c => c.Feedback)
                  .HasForeignKey<Feedback>(f => f.ComplaintId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.User)
                  .WithMany()
                  .HasForeignKey(f => f.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── AuditLog ────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.AuditId);
        });
    }
}