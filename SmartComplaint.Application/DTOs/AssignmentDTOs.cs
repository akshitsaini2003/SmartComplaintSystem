namespace SmartComplaint.Application.DTOs;

public class CreateAssignmentDto
{
    public int ComplaintId { get; set; }
    public int AgentId { get; set; }
}

public class AssignmentResponseDto
{
    public int AssignmentId { get; set; }
    public int ComplaintId { get; set; }
    public string ComplaintTitle { get; set; } = string.Empty;
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}

public class ReassignDto
{
    public int NewAgentId { get; set; }
}