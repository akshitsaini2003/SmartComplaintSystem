namespace SmartComplaint.Web.Models;

// ─── User Models ──────────────────────────────────────────
public class UserModel
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateUserModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}

public class UpdateUserModel
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpdateRoleModel
{
    public string Role { get; set; } = string.Empty;
}

// ─── Assignment Models ────────────────────────────────────
public class AssignmentModel
{
    public int AssignmentId { get; set; }
    public int ComplaintId { get; set; }
    public string ComplaintTitle { get; set; } = string.Empty;
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}

public class CreateAssignmentModel
{
    public int ComplaintId { get; set; }
    public int AgentId { get; set; }
}

public class ReassignModel
{
    public int NewAgentId { get; set; }
}

// ─── Feedback Models ──────────────────────────────────────
public class FeedbackModel
{
    public int FeedbackId { get; set; }
    public int ComplaintId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateFeedbackModel
{
    public int ComplaintId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class FeedbackReportModel
{
    public double AverageRating { get; set; }
    public int TotalFeedbacks { get; set; }
}