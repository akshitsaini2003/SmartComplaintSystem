namespace SmartComplaint.Application.DTOs;

public class AdminDashboardDto
{
    public int TotalComplaints { get; set; }
    public int OpenComplaints { get; set; }
    public int InProgressComplaints { get; set; }
    public int ResolvedToday { get; set; }
    public int ClosedComplaints { get; set; }
    public double AverageResolutionHours { get; set; }
    public int SlaBreaches { get; set; }
    public List<CategoryStatsDto> ComplaintsByCategory { get; set; } = new();
    public List<PriorityStatsDto> ComplaintsByPriority { get; set; } = new();
    public List<AgentStatsDto> TopAgents { get; set; } = new();
}

public class AgentDashboardDto
{
    public int TotalAssigned { get; set; }
    public int PendingComplaints { get; set; }
    public int ResolvedComplaints { get; set; }
    public double AverageResolutionHours { get; set; }
}

public class UserDashboardDto
{
    public int TotalComplaints { get; set; }
    public int OpenComplaints { get; set; }
    public int ResolvedComplaints { get; set; }
    public double AverageFeedbackRating { get; set; }
}

public class CategoryStatsDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PriorityStatsDto
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AgentStatsDto
{
    public string AgentName { get; set; } = string.Empty;
    public int ResolvedCount { get; set; }
}

public class ReportFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public int? AgentId { get; set; }
}

public class ReportResultDto
{
    public int TotalComplaints { get; set; }
    public int Resolved { get; set; }
    public int Pending { get; set; }
    public double AverageResolutionHours { get; set; }
    public List<ComplaintListDto> Complaints { get; set; } = new();
}