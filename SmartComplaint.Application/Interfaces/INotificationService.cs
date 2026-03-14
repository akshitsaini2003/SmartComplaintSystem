using SmartComplaint.Application.DTOs;

namespace SmartComplaint.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponseDto>> GetMyAsync(int userId);
    Task<string> MarkReadAsync(int notificationId, int userId);
    Task<string> MarkAllReadAsync(int userId);
    Task<string> DeleteAsync(int notificationId, int userId);
}