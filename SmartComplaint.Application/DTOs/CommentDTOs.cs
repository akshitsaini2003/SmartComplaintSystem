namespace SmartComplaint.Application.DTOs;

public class CreateCommentDto
{
    public int ComplaintId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CommentResponseDto
{
    public int CommentId { get; set; }
    public int ComplaintId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}