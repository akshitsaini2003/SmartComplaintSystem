using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Domain.Enums;
using SmartComplaint.Infrastructure.Data;

namespace SmartComplaint.Infrastructure.Services;

public class SlaMonitorService : ISlaMonitorService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public SlaMonitorService(
        AppDbContext context,
        IEmailService emailService,
        IConfiguration config)
    {
        _context = context;
        _emailService = emailService;
        _config = config;
    }

    public async Task CheckAndProcessBreachesAsync()
    {
        // ── Fetch SLA limits from config ──────────────────
        var highHours = _config.GetValue<int>("SLASettings:HighPriorityHours");
        var mediumHours = _config.GetValue<int>("SLASettings:MediumPriorityHours");
        var lowHours = _config.GetValue<int>("SLASettings:LowPriorityHours");

        var now = DateTime.UtcNow;

        // ── Fetch all open/inProgress complaints ──────────
        var activeComplaints = await _context.Complaints
            .Where(c => !c.IsDeleted &&
                        (c.Status == ComplaintStatus.Open ||
                         c.Status == ComplaintStatus.InProgress))
            .ToListAsync();

        // ── Fetch admin email ─────────────────────────────
        var admin = await _context.Users
            .FirstOrDefaultAsync(u => u.Role == Domain.Enums.UserRole.Admin);

        foreach (var complaint in activeComplaints)
        {
            var slaHours = complaint.Priority switch
            {
                Priority.High => highHours,
                Priority.Medium => mediumHours,
                Priority.Low => lowHours,
                _ => lowHours
            };

            var ageHours = (now - complaint.CreatedDate).TotalHours;

            if (ageHours >= slaHours)
            {
                // ── Send admin email alert ─────────────────
                if (admin != null)
                {
                    await _emailService.SendSlaBreachEmailAsync(
                        admin.Email,
                        complaint.Title,
                        complaint.Priority.ToString());
                }

                // ── Create in-app notification for admin ───
                if (admin != null)
                {
                    var alreadyNotified = await _context.Notifications
                        .AnyAsync(n =>
                            n.UserId == admin.UserId &&
                            n.Message.Contains($"SLA breached") &&
                            n.Message.Contains(complaint.ComplaintId.ToString()));

                    if (!alreadyNotified)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = admin.UserId,
                            Message = $"SLA breached for Complaint #{complaint.ComplaintId}: " +
                                      $"'{complaint.Title}' ({complaint.Priority} priority)",
                        });
                    }
                }

                // ── Log to AuditLogs ───────────────────────
                var alreadyLogged = await _context.AuditLogs
                    .AnyAsync(a =>
                        a.TableName == "Complaints" &&
                        a.RecordId == complaint.ComplaintId.ToString() &&
                        a.Action == "SLA_BREACH");

                if (!alreadyLogged)
                {
                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = admin?.UserId,
                        Action = "SLA_BREACH",
                        TableName = "Complaints",
                        RecordId = complaint.ComplaintId.ToString(),
                        Timestamp = now,
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}