using Microsoft.EntityFrameworkCore;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Domain.Enums;
using SmartComplaint.Infrastructure.Data;

namespace SmartComplaint.Infrastructure.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context;

    public AssignmentService(IUnitOfWork uow,
        IEmailService emailService, AppDbContext context)
    {
        _uow = uow;
        _emailService = emailService;
        _context = context;
    }

    public async Task<AssignmentResponseDto> AssignAsync(CreateAssignmentDto dto)
    {
        // Validate complaint
        var complaint = await _uow.Complaints.GetByIdAsync(dto.ComplaintId);
        if (complaint == null || complaint.IsDeleted)
            throw new Exception("Complaint not found.");

        // Validate agent
        var agent = await _uow.Users.GetByIdAsync(dto.AgentId);
        if (agent == null || agent.Role != UserRole.Agent)
            throw new Exception("Invalid agent.");

        var assignment = new ComplaintAssignment
        {
            ComplaintId = dto.ComplaintId,
            AgentId = dto.AgentId,
            AssignedDate = DateTime.UtcNow,
        };

        await _uow.ComplaintAssignments.AddAsync(assignment);
        await _uow.SaveChangesAsync();

        // Notify agent via email
        await _emailService.SendComplaintAssignedEmailAsync(
            agent.Email, agent.Name, complaint.Title);

        // Create in-app notification for agent
        await _uow.Notifications.AddAsync(new Notification
        {
            UserId = agent.UserId,
            Message = $"Complaint '{complaint.Title}' has been assigned to you.",
        });
        await _uow.SaveChangesAsync();

        return await MapToDto(assignment.AssignmentId);
    }

    public async Task<IEnumerable<AssignmentResponseDto>> GetByAgentAsync(int agentId)
    {
        var assignments = await _context.ComplaintAssignments
            .Include(a => a.Complaint)
            .Include(a => a.Agent)
            .Where(a => a.AgentId == agentId)
            .ToListAsync();

        return assignments.Select(a => new AssignmentResponseDto
        {
            AssignmentId = a.AssignmentId,
            ComplaintId = a.ComplaintId,
            ComplaintTitle = a.Complaint.Title,
            AgentId = a.AgentId,
            AgentName = a.Agent.Name,
            AssignedDate = a.AssignedDate,
        });
    }

    public async Task<AssignmentResponseDto> ReassignAsync(int assignmentId, int newAgentId)
    {
        var assignment = await _uow.ComplaintAssignments.GetByIdAsync(assignmentId);
        if (assignment == null) throw new Exception("Assignment not found.");

        var agent = await _uow.Users.GetByIdAsync(newAgentId);
        if (agent == null || agent.Role != UserRole.Agent)
            throw new Exception("Invalid agent.");

        assignment.AgentId = newAgentId;
        assignment.AssignedDate = DateTime.UtcNow;

        _uow.ComplaintAssignments.Update(assignment);
        await _uow.SaveChangesAsync();

        return await MapToDto(assignmentId);
    }

    private async Task<AssignmentResponseDto> MapToDto(int assignmentId)
    {
        var a = await _context.ComplaintAssignments
            .Include(x => x.Complaint)
            .Include(x => x.Agent)
            .FirstAsync(x => x.AssignmentId == assignmentId);

        return new AssignmentResponseDto
        {
            AssignmentId = a.AssignmentId,
            ComplaintId = a.ComplaintId,
            ComplaintTitle = a.Complaint.Title,
            AgentId = a.AgentId,
            AgentName = a.Agent.Name,
            AssignedDate = a.AssignedDate,
        };
    }
}