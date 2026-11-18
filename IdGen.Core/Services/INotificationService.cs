using IdGen.Core.Models;

namespace IdGen.Core.Services;

public interface INotificationService
{
    Task<Notification> SendNotificationAsync(int userId, string message);
    Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(int userId);
    Task MarkAsReadAsync(int notificationId);
}

public interface IEmailNotificationService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public interface ISmsNotificationService
{
    Task SendSmsAsync(string toPhone, string message);
}
