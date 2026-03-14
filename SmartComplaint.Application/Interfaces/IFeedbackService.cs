using SmartComplaint.Application.DTOs;

namespace SmartComplaint.Application.Interfaces;

public interface IFeedbackService
{
    Task<FeedbackResponseDto> SubmitAsync(CreateFeedbackDto dto, int userId);
    Task<FeedbackResponseDto> GetByComplaintAsync(int complaintId);
    Task<FeedbackReportDto> GetReportAsync();
}