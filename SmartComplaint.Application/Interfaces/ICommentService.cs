using SmartComplaint.Application.DTOs;

namespace SmartComplaint.Application.Interfaces;

public interface ICommentService
{
    Task<CommentResponseDto> AddAsync(CreateCommentDto dto, int userId);
    Task<IEnumerable<CommentResponseDto>> GetByComplaintAsync(int complaintId);
    Task<string> DeleteAsync(int commentId, int userId, string role);
}