using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Data.Repositories;
using Xunit;

namespace IdGen.Tests;

public class NotificationRepositoryTests
{
    [Fact]
    public async Task CreateAndMarkNotificationRead_WorksCorrectly()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var repo = new NotificationRepository(factory);

        var user = new User
        {
            Role = UserRole.Student,
            Username = "notifUser",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            StudentNo = "2024-1111",
            LastName = "Alert",
            FirstName = "Student",
            Email = "alert@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Approved
        };
        await userRepo.CreateAsync(user);

        var notif = new Notification
        {
            UserId = user.Id,
            Message = "Your registration has been approved!",
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        int notifId = await repo.CreateAsync(notif);
        Assert.True(notifId > 0);

        var list = await repo.GetByUserIdAsync(user.Id);
        Assert.Single(list);
        Assert.False(list[0].IsRead);
        Assert.Equal("Your registration has been approved!", list[0].Message);

        // Mark as read
        await repo.MarkAsReadAsync(notifId);

        var updatedList = await repo.GetByUserIdAsync(user.Id);
        Assert.Single(updatedList);
        Assert.True(updatedList[0].IsRead);
    }
}
