using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;

namespace SmartComplaint.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;

    public NotificationService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<NotificationResponseDto>> GetMyAsync(int userId)
    {
        var notifications = await _uow.Notifications
            .FindAsync(n => n.UserId == userId && !n.IsDeleted);

        return notifications
            .OrderByDescending(n => n.CreatedDate)
            .Select(n => new NotificationResponseDto
            {
                NotificationId = n.NotificationId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedDate = n.CreatedDate,
            });
    }

    public async Task<string> MarkReadAsync(int notificationId, int userId)
    {
        var notification = await _uow.Notifications.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId)
            throw new Exception("Notification not found.");

        notification.IsRead = true;
        _uow.Notifications.Update(notification);
        await _uow.SaveChangesAsync();

        return "Marked as read.";
    }

    public async Task<string> MarkAllReadAsync(int userId)
    {
        var notifications = await _uow.Notifications
            .FindAsync(n => n.UserId == userId && !n.IsRead);

        foreach (var n in notifications)
        {
            n.IsRead = true;
            _uow.Notifications.Update(n);
        }

        await _uow.SaveChangesAsync();
        return "All notifications marked as read.";
    }

    public async Task<string> DeleteAsync(int notificationId, int userId)
    {
        var notification = await _uow.Notifications.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId)
            throw new Exception("Notification not found.");

        notification.IsDeleted = true;
        _uow.Notifications.Update(notification);
        await _uow.SaveChangesAsync();

        return "Notification deleted.";
    }
}