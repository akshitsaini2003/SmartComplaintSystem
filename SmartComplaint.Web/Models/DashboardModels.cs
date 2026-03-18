namespace SmartComplaint.Web.Models;

public class AdminDashboardModel
{
    public int TotalComplaints { get; set; }
    public int OpenComplaints { get; set; }
    public int InProgressComplaints { get; set; }
    public int ResolvedToday { get; set; }
    public int ClosedComplaints { get; set; }
    public double AverageResolutionHours { get; set; }
    public int SlaBreaches { get; set; }
    public List<CategoryStatsModel> ComplaintsByCategory { get; set; } = new();
    public List<PriorityStatsModel> ComplaintsByPriority { get; set; } = new();
    public List<AgentStatsModel> TopAgents { get; set; } = new();
}

public class AgentDashboardModel
{
    public int TotalAssigned { get; set; }
    public int PendingComplaints { get; set; }
    public int ResolvedComplaints { get; set; }
    public double AverageResolutionHours { get; set; }
}

public class UserDashboardModel
{
    public int TotalComplaints { get; set; }
    public int OpenComplaints { get; set; }
    public int ResolvedComplaints { get; set; }
    public double AverageFeedbackRating { get; set; }
}

public class CategoryStatsModel
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PriorityStatsModel
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AgentStatsModel
{
    public string AgentName { get; set; } = string.Empty;
    public int ResolvedCount { get; set; }
}

public class ReportFilterModel
{
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public int? AgentId { get; set; }
}

public class ReportResultModel
{
    public int TotalComplaints { get; set; }
    public int Resolved { get; set; }
    public int Pending { get; set; }
    public double AverageResolutionHours { get; set; }
    public List<ComplaintListModel> Complaints { get; set; } = new();
}