namespace SmartComplaint.Application.DTOs;

public class CreateFeedbackDto
{
    public int ComplaintId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}

public class FeedbackResponseDto
{
    public int FeedbackId { get; set; }
    public int ComplaintId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class FeedbackReportDto
{
    public double AverageRating { get; set; }
    public int TotalFeedbacks { get; set; }
}