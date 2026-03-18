using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Domain.Enums;
using SmartComplaint.Infrastructure.Data;

namespace SmartComplaint.Infrastructure.Services;

public class ComplaintService : IComplaintService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context;

    public ComplaintService(IUnitOfWork uow, IMapper mapper,
        IEmailService emailService, AppDbContext context)
    {
        _uow = uow;
        _mapper = mapper;
        _emailService = emailService;
        _context = context;
    }

    // ─── Create ──────────────────────────────────────────────
    public async Task<ComplaintResponseDto> CreateAsync(CreateComplaintDto dto, int userId)
    {
        var priority = DetectPriority(dto.Title, dto.Description, dto.Priority);

        var complaint = new Complaint
        {
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            UserId = userId,
            Priority = priority,
            Status = ComplaintStatus.Open,
        };

        await _uow.Complaints.AddAsync(complaint);
        await _uow.SaveChangesAsync();

        // Send email to user
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user != null)
            await _emailService.SendComplaintCreatedEmailAsync(
                user.Email, user.Name, dto.Title);

        return await GetByIdAsync(complaint.ComplaintId);
    }

    // ─── Get All (Admin/Agent) ────────────────────────────────
    public async Task<PagedResult<ComplaintListDto>> GetAllAsync(
        int page, int pageSize, string? status, string? priority)
    {
        var query = _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.User)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrEmpty(status) &&
            Enum.TryParse<ComplaintStatus>(status, true, out var s))
            query = query.Where(c => c.Status == s);

        if (!string.IsNullOrEmpty(priority) &&
            Enum.TryParse<Priority>(priority, true, out var p))
            query = query.Where(c => c.Priority == p);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ComplaintListDto>
        {
            Items = _mapper.Map<List<ComplaintListDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    // ─── Get By Id ───────────────────────────────────────────
    public async Task<ComplaintResponseDto> GetByIdAsync(int id)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.ComplaintId == id && !c.IsDeleted);

        if (complaint == null) throw new Exception("Complaint not found.");

        return _mapper.Map<ComplaintResponseDto>(complaint);
    }

    // ─── Update ──────────────────────────────────────────────
    public async Task<ComplaintResponseDto> UpdateAsync(
        int id, UpdateComplaintDto dto, int userId)
    {
        var complaint = await _uow.Complaints.GetByIdAsync(id);
        if (complaint == null || complaint.IsDeleted)
            throw new Exception("Complaint not found.");

        complaint.Title = dto.Title;
        complaint.Description = dto.Description;
        complaint.CategoryId = dto.CategoryId;

        _uow.Complaints.Update(complaint);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    // ─── Soft Delete ─────────────────────────────────────────
    public async Task<string> DeleteAsync(int id)
    {
        var complaint = await _uow.Complaints.GetByIdAsync(id);
        if (complaint == null || complaint.IsDeleted)
            throw new Exception("Complaint not found.");

        complaint.IsDeleted = true;
        _uow.Complaints.Update(complaint);
        await _uow.SaveChangesAsync();

        return "Complaint deleted successfully.";
    }

    // ─── My Complaints ───────────────────────────────────────
    public async Task<PagedResult<ComplaintListDto>> GetMyComplaintsAsync(
        int userId, int page, int pageSize)
    {
        var query = _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.User)
            .Where(c => c.UserId == userId && !c.IsDeleted);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ComplaintListDto>
        {
            Items = _mapper.Map<List<ComplaintListDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    // ─── Update Status ───────────────────────────────────────
    public async Task<string> UpdateStatusAsync(int id, UpdateStatusDto dto, string changedBy)
    {
        var complaint = await _uow.Complaints.GetByIdAsync(id);
        if (complaint == null || complaint.IsDeleted)
            throw new Exception("Complaint not found.");

        if (!Enum.TryParse<ComplaintStatus>(dto.Status, true, out var newStatus))
            throw new Exception("Invalid status value.");

        // Validate transition
        if (!IsValidTransition(complaint.Status, newStatus))
            throw new Exception(
                $"Invalid transition: {complaint.Status} → {newStatus}");

        var oldStatus = complaint.Status;
        complaint.Status = newStatus;
        _uow.Complaints.Update(complaint);

        // Log history
        await _uow.ComplaintHistories.AddAsync(new ComplaintHistory
        {
            ComplaintId = id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = changedBy,
        });

        await _uow.SaveChangesAsync();

        // Notify user via email
        var user = await _uow.Users.GetByIdAsync(complaint.UserId);
        if (user != null)
            await _emailService.SendStatusChangedEmailAsync(
                user.Email, user.Name, complaint.Title, newStatus.ToString());

        return $"Status updated to {newStatus}.";
    }

    // ─── Smart Priority Detection ────────────────────────────
    private Priority DetectPriority(string title, string description, string? manualPriority)
    {
        if (!string.IsNullOrEmpty(manualPriority) &&
            Enum.TryParse<Priority>(manualPriority, true, out var parsed))
            return parsed;

        var text = $"{title} {description}".ToLower();

        var highKeywords = new[] { "server down", "not working", "emergency", "critical", "outage" };
        var mediumKeywords = new[] { "slow", "intermittent", "degraded", "delay" };

        if (highKeywords.Any(k => text.Contains(k))) return Priority.High;
        if (mediumKeywords.Any(k => text.Contains(k))) return Priority.Medium;

        return Priority.Low;
    }

    // ─── Status Transition Validator ─────────────────────────
    private bool IsValidTransition(ComplaintStatus current, ComplaintStatus next)
    {
        var allowed = new Dictionary<ComplaintStatus, List<ComplaintStatus>>
        {
            { ComplaintStatus.Open,       new() { ComplaintStatus.InProgress } },
            { ComplaintStatus.InProgress, new() { ComplaintStatus.OnHold, ComplaintStatus.Resolved } },
            { ComplaintStatus.OnHold,     new() { ComplaintStatus.InProgress } },
            { ComplaintStatus.Resolved,   new() { ComplaintStatus.Closed } },
            { ComplaintStatus.Closed,     new() { } },
        };

        return allowed.ContainsKey(current) && allowed[current].Contains(next);
    }

    //public async Task<IEnumerable<ComplaintHistory>> GetHistoryAsync(int complaintId)
    //{
    //    return await _uow.ComplaintHistories
    //        .FindAsync(h => h.ComplaintId == complaintId);
    //}
    public async Task<IEnumerable<ComplaintHistoryDto>> GetHistoryAsync(int complaintId)
    {
        var histories = await _uow.ComplaintHistories
            .FindAsync(h => h.ComplaintId == complaintId);

        return histories
            .OrderBy(h => h.ChangedDate)
            .Select(h => new ComplaintHistoryDto
            {
                HistoryId = h.HistoryId,
                ComplaintId = h.ComplaintId,
                OldStatus = h.OldStatus.ToString(), // ✅ enum → string
                NewStatus = h.NewStatus.ToString(), // ✅ enum → string
                ChangedDate = h.ChangedDate,
                ChangedBy = h.ChangedBy,
            });
    }
}