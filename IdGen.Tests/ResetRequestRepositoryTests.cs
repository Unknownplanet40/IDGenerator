using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Data.Repositories;
using Xunit;

namespace IdGen.Tests;

public class ResetRequestRepositoryTests
{
    [Fact]
    public async Task CreateAndResolveResetRequest_WorksCorrectly()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var repo = new ResetRequestRepository(factory);

        var user = new User
        {
            Role = UserRole.Student,
            Username = "studentReq",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            StudentNo = "2024-3333",
            LastName = "Lee",
            FirstName = "Bruce",
            Email = "bruce@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Approved
        };
        await userRepo.CreateAsync(user);

        var request = new ResetRequest
        {
            UserId = user.Id,
            Status = ResetStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        int id = await repo.CreateAsync(request);
        Assert.True(id > 0);

        var pending = await repo.GetPendingByUserIdAsync(user.Id);
        Assert.NotNull(pending);
        Assert.Equal(ResetStatus.Pending, pending.Status);

        // Resolve request
        var resolvedTime = DateTime.UtcNow;
        await repo.UpdateStatusAsync(id, ResetStatus.Approved, resolvedTime);

        var latest = await repo.GetLatestByUserIdAsync(user.Id);
        Assert.NotNull(latest);
        Assert.Equal(ResetStatus.Approved, latest.Status);
        Assert.NotNull(latest.ResolvedAt);

        // No longer pending
        var noPending = await repo.GetPendingByUserIdAsync(user.Id);
        Assert.Null(noPending);
    }
}
