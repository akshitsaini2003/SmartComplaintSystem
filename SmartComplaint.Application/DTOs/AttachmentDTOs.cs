namespace SmartComplaint.Application.DTOs;

public class AttachmentResponseDto
{
    public int AttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
}