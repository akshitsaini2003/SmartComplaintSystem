using SmartComplaint.Application.DTOs;
using SmartComplaint.Domain.Entities;

namespace SmartComplaint.Application.Interfaces;

public interface IComplaintService
{
    Task<ComplaintResponseDto> CreateAsync(CreateComplaintDto dto, int userId);
    Task<PagedResult<ComplaintListDto>> GetAllAsync(int page, int pageSize, string? status, string? priority);
    Task<ComplaintResponseDto> GetByIdAsync(int id);
    Task<ComplaintResponseDto> UpdateAsync(int id, UpdateComplaintDto dto, int userId);
    Task<string> DeleteAsync(int id);
    Task<PagedResult<ComplaintListDto>> GetMyComplaintsAsync(int userId, int page, int pageSize);
    Task<string> UpdateStatusAsync(int id, UpdateStatusDto dto, string changedBy);
    //Task<IEnumerable<ComplaintHistory>> GetHistoryAsync(int complaintId);

    Task<IEnumerable<ComplaintHistoryDto>> GetHistoryAsync(int complaintId);
}