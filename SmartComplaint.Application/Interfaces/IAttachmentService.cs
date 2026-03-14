using Microsoft.AspNetCore.Http;
using SmartComplaint.Application.DTOs;

namespace SmartComplaint.Application.Interfaces;

public interface IAttachmentService
{
    Task<AttachmentResponseDto> UploadAsync(int complaintId, IFormFile file);
    Task<IEnumerable<AttachmentResponseDto>> GetByComplaintAsync(int complaintId);
    Task<string> DeleteAsync(int attachmentId);
}