using Microsoft.EntityFrameworkCore;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Enums;
using SmartComplaint.Infrastructure.Data;

namespace SmartComplaint.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context) => _context = context;

    // ─── Admin Dashboard ─────────────────────────────────────
    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var complaints = await _context.Complaints
            .Include(c => c.Category)
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;

        // Resolved complaints with history for avg resolution time
        var resolvedComplaints = complaints
            .Where(c => c.Status == ComplaintStatus.Resolved ||
                        c.Status == ComplaintStatus.Closed)
            .ToList();

        // Avg resolution hours
        var histories = await _context.ComplaintHistories
            .Where(h => h.NewStatus == ComplaintStatus.Resolved)
            .ToListAsync();

        double avgResolution = 0;
        if (histories.Any())
        {
            var resolutionTimes = histories.Select(h =>
            {
                var complaint = complaints
                    .FirstOrDefault(c => c.ComplaintId == h.ComplaintId);
                return complaint != null
                    ? (h.ChangedDate - complaint.CreatedDate).TotalHours
                    : 0;
            }).Where(t => t > 0);

            if (resolutionTimes.Any())
                avgResolution = Math.Round(resolutionTimes.Average(), 2);
        }

        // SLA breaches from audit logs
        var slaBreaches = await _context.AuditLogs
            .CountAsync(a => a.Action == "SLA_BREACH");

        // Top agents
        var topAgents = await _context.ComplaintAssignments
            .Include(a => a.Agent)
            .Include(a => a.Complaint)
            .Where(a => a.Complaint.Status == ComplaintStatus.Resolved ||
                        a.Complaint.Status == ComplaintStatus.Closed)
            .GroupBy(a => new { a.AgentId, a.Agent.Name })
            .Select(g => new AgentStatsDto
            {
                AgentName = g.Key.Name,
                ResolvedCount = g.Count(),
            })
            .OrderByDescending(a => a.ResolvedCount)
            .Take(5)
            .ToListAsync();

        return new AdminDashboardDto
        {
            TotalComplaints = complaints.Count,
            OpenComplaints = complaints.Count(c => c.Status == ComplaintStatus.Open),
            InProgressComplaints = complaints.Count(c => c.Status == ComplaintStatus.InProgress),
            ResolvedToday = complaints.Count(c =>
                (c.Status == ComplaintStatus.Resolved ||
                 c.Status == ComplaintStatus.Closed) &&
                c.CreatedDate.Date == today),
            ClosedComplaints = complaints.Count(c => c.Status == ComplaintStatus.Closed),
            AverageResolutionHours = avgResolution,
            SlaBreaches = slaBreaches,
            ComplaintsByCategory = complaints
                .GroupBy(c => c.Category.Name)
                .Select(g => new CategoryStatsDto
                {
                    CategoryName = g.Key,
                    Count = g.Count(),
                }).ToList(),
            ComplaintsByPriority = complaints
                .GroupBy(c => c.Priority.ToString())
                .Select(g => new PriorityStatsDto
                {
                    Priority = g.Key,
                    Count = g.Count(),
                }).ToList(),
            TopAgents = topAgents,
        };
    }

    // ─── Agent Dashboard ─────────────────────────────────────
    public async Task<AgentDashboardDto> GetAgentDashboardAsync(int agentId)
    {
        var assignments = await _context.ComplaintAssignments
            .Include(a => a.Complaint)
            .Where(a => a.AgentId == agentId && !a.Complaint.IsDeleted)
            .ToListAsync();

        var complaints = assignments.Select(a => a.Complaint).ToList();

        var resolved = complaints
            .Where(c => c.Status == ComplaintStatus.Resolved ||
                        c.Status == ComplaintStatus.Closed)
            .ToList();

        // Avg resolution hours for this agent
        var histories = await _context.ComplaintHistories
            .Where(h => h.NewStatus == ComplaintStatus.Resolved &&
                        complaints.Select(c => c.ComplaintId).Contains(h.ComplaintId))
            .ToListAsync();

        double avgResolution = 0;
        if (histories.Any())
        {
            var times = histories.Select(h =>
            {
                var c = complaints.FirstOrDefault(x => x.ComplaintId == h.ComplaintId);
                return c != null ? (h.ChangedDate - c.CreatedDate).TotalHours : 0;
            }).Where(t => t > 0);

            if (times.Any())
                avgResolution = Math.Round(times.Average(), 2);
        }

        return new AgentDashboardDto
        {
            TotalAssigned = complaints.Count,
            PendingComplaints = complaints.Count(c =>
                c.Status == ComplaintStatus.Open ||
                c.Status == ComplaintStatus.InProgress ||
                c.Status == ComplaintStatus.OnHold),
            ResolvedComplaints = resolved.Count,
            AverageResolutionHours = avgResolution,
        };
    }

    // ─── User Dashboard ──────────────────────────────────────
    public async Task<UserDashboardDto> GetUserDashboardAsync(int userId)
    {
        var complaints = await _context.Complaints
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync();

        var feedbacks = await _context.Feedbacks
            .Where(f => f.UserId == userId)
            .ToListAsync();

        return new UserDashboardDto
        {
            TotalComplaints = complaints.Count,
            OpenComplaints = complaints.Count(c =>
                c.Status == ComplaintStatus.Open ||
                c.Status == ComplaintStatus.InProgress ||
                c.Status == ComplaintStatus.OnHold),
            ResolvedComplaints = complaints.Count(c =>
                c.Status == ComplaintStatus.Resolved ||
                c.Status == ComplaintStatus.Closed),
            AverageFeedbackRating = feedbacks.Any()
                ? Math.Round(feedbacks.Average(f => f.Rating), 2)
                : 0,
        };
    }

    // ─── Reports with Filters ────────────────────────────────
    public async Task<ReportResultDto> GetReportAsync(ReportFilterDto filter)
    {
        var query = _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.User)
            .Where(c => !c.IsDeleted);

        if (filter.FromDate.HasValue)
            query = query.Where(c => c.CreatedDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(c => c.CreatedDate <= filter.ToDate.Value);

        if (!string.IsNullOrEmpty(filter.Priority) &&
            Enum.TryParse<Priority>(filter.Priority, true, out var p))
            query = query.Where(c => c.Priority == p);

        if (!string.IsNullOrEmpty(filter.Status) &&
            Enum.TryParse<ComplaintStatus>(filter.Status, true, out var s))
            query = query.Where(c => c.Status == s);

        if (!string.IsNullOrEmpty(filter.Category))
            query = query.Where(c => c.Category.Name
                .ToLower().Contains(filter.Category.ToLower()));

        if (filter.AgentId.HasValue)
        {
            var assignedComplaintIds = await _context.ComplaintAssignments
                .Where(a => a.AgentId == filter.AgentId.Value)
                .Select(a => a.ComplaintId)
                .ToListAsync();
            query = query.Where(c => assignedComplaintIds.Contains(c.ComplaintId));
        }

        var complaints = await query
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();

        return new ReportResultDto
        {
            TotalComplaints = complaints.Count,
            Resolved = complaints.Count(c =>
                c.Status == ComplaintStatus.Resolved ||
                c.Status == ComplaintStatus.Closed),
            Pending = complaints.Count(c =>
                c.Status == ComplaintStatus.Open ||
                c.Status == ComplaintStatus.InProgress ||
                c.Status == ComplaintStatus.OnHold),
            AverageResolutionHours = 0, // calculated separately if needed
            Complaints = complaints.Select(c => new ComplaintListDto
            {
                ComplaintId = c.ComplaintId,
                Title = c.Title,
                Priority = c.Priority.ToString(),
                Status = c.Status.ToString(),
                CategoryName = c.Category.Name,
                CreatedDate = c.CreatedDate,
            }).ToList(),
        };
    }
}