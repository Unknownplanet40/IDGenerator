using IdGen.Core.Models;
using IdGen.Core.Services;
using IdGen.Data.Repositories;

namespace IdGen.Data.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Notification> SendNotificationAsync(int userId, string message)
    {
        var notif = new Notification
        {
            UserId = userId,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        int id = await _notificationRepository.CreateAsync(notif);
        notif.Id = id;
        return notif;
    }

    public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(int userId)
    {
        return await _notificationRepository.GetByUserIdAsync(userId);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await _notificationRepository.MarkAsReadAsync(notificationId);
    }
}
