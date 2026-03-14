using SmartComplaint.Application.DTOs;

namespace SmartComplaint.Application.Interfaces;

public interface IAssignmentService
{
    Task<AssignmentResponseDto> AssignAsync(CreateAssignmentDto dto);
    Task<IEnumerable<AssignmentResponseDto>> GetByAgentAsync(int agentId);
    Task<AssignmentResponseDto> ReassignAsync(int assignmentId, int newAgentId);
}