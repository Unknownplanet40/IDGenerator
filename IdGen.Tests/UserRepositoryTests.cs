using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Data.Repositories;
using Microsoft.Data.Sqlite;
using Xunit;

namespace IdGen.Tests;

public class UserRepositoryTests
{
    [Fact]
    public async Task HasAnyAdminAsync_ReturnsFalseWhenEmpty_AndTrueWhenAdminExists()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new UserRepository(factory);

        Assert.False(await repo.HasAnyAdminAsync());

        var admin = new User
        {
            Role = UserRole.Admin,
            Username = "admin",
            PasswordHash = "hash123",
            Salt = "salt123",
            Iterations = 600_000,
            LastName = "Admin",
            FirstName = "Super",
            Email = "admin@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Approved
        };

        await repo.CreateAsync(admin);

        Assert.True(await repo.HasAnyAdminAsync());
    }

    [Fact]
    public async Task GetByUsernameAsync_IsCaseInsensitive()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new UserRepository(factory);

        var user = new User
        {
            Role = UserRole.Student,
            Username = "JuanDelaCruz",
            PasswordHash = "hash",
            Salt = "salt",
            Iterations = 600_000,
            StudentNo = "2024-0001",
            LastName = "Dela Cruz",
            FirstName = "Juan",
            Email = "juan@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Pending
        };

        await repo.CreateAsync(user);

        var retrievedLower = await repo.GetByUsernameAsync("juandelacruz");
        Assert.NotNull(retrievedLower);
        Assert.Equal(user.Id, retrievedLower.Id);

        var retrievedUpper = await repo.GetByUsernameAsync("JUANDELACRUZ");
        Assert.NotNull(retrievedUpper);
        Assert.Equal(user.Id, retrievedUpper.Id);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsername_ThrowsSqliteException()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new UserRepository(factory);

        var user1 = new User
        {
            Role = UserRole.Student,
            Username = "student1",
            PasswordHash = "hash1",
            Salt = "salt1",
            Iterations = 600_000,
            StudentNo = "2024-0001",
            LastName = "One",
            FirstName = "Student",
            Email = "s1@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Pending
        };
        await repo.CreateAsync(user1);

        var user2 = new User
        {
            Role = UserRole.Student,
            Username = "STUDENT1", // Case insensitive collision
            PasswordHash = "hash2",
            Salt = "salt2",
            Iterations = 600_000,
            StudentNo = "2024-0002",
            LastName = "Two",
            FirstName = "Student",
            Email = "s2@school.edu",
            Phone = "09171234568",
            Status = RegistrationStatus.Pending
        };

        await Assert.ThrowsAsync<SqliteException>(() => repo.CreateAsync(user2));
    }

    [Fact]
    public async Task CreateAsync_DuplicateStudentNo_ThrowsSqliteException()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new UserRepository(factory);

        var user1 = new User
        {
            Role = UserRole.Student,
            Username = "userA",
            PasswordHash = "hash1",
            Salt = "salt1",
            Iterations = 600_000,
            StudentNo = "2024-SAME",
            LastName = "One",
            FirstName = "Student",
            Email = "s1@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Pending
        };
        await repo.CreateAsync(user1);

        var user2 = new User
        {
            Role = UserRole.Student,
            Username = "userB",
            PasswordHash = "hash2",
            Salt = "salt2",
            Iterations = 600_000,
            StudentNo = "2024-SAME",
            LastName = "Two",
            FirstName = "Student",
            Email = "s2@school.edu",
            Phone = "09171234568",
            Status = RegistrationStatus.Pending
        };

        await Assert.ThrowsAsync<SqliteException>(() => repo.CreateAsync(user2));
    }

    [Fact]
    public async Task UpdateStatusAndRetrieve_WorksCorrectly()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new UserRepository(factory);

        var user = new User
        {
            Role = UserRole.Student,
            Username = "pendingStudent",
            PasswordHash = "hash",
            Salt = "salt",
            Iterations = 600_000,
            StudentNo = "2024-7777",
            LastName = "Santos",
            FirstName = "Maria",
            Email = "maria@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Pending
        };
        await repo.CreateAsync(user);

        var pendingList = await repo.GetPendingStudentsAsync();
        Assert.Single(pendingList);

        await repo.UpdateStatusAsync(user.Id, RegistrationStatus.Approved);

        var updated = await repo.GetByIdAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal(RegistrationStatus.Approved, updated.Status);

        var pendingAfter = await repo.GetPendingStudentsAsync();
        Assert.Empty(pendingAfter);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsBothAdminsAndStudents()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new UserRepository(factory);

        var admin = new User
        {
            Role = UserRole.Admin,
            Username = "sysadmin",
            PasswordHash = "hashAdmin",
            Salt = "saltAdmin",
            Iterations = 600_000,
            LastName = "Admin",
            FirstName = "Lead",
            Email = "admin@cvsu.edu.ph",
            Phone = "09170000000",
            Status = RegistrationStatus.Approved
        };
        await repo.CreateAsync(admin);

        var student = new User
        {
            Role = UserRole.Student,
            Username = "studentuser",
            PasswordHash = "hashStudent",
            Salt = "saltStudent",
            Iterations = 600_000,
            StudentNo = "202110055",
            LastName = "Capadocia",
            FirstName = "Ryan James",
            Email = "ryan@cvsu.edu.ph",
            Phone = "09171111111",
            Status = RegistrationStatus.Approved
        };
        await repo.CreateAsync(student);

        var allUsers = await repo.GetAllUsersAsync();
        Assert.Equal(2, allUsers.Count);
        Assert.Contains(allUsers, u => u.Role == UserRole.Admin && u.Username == "sysadmin");
        Assert.Contains(allUsers, u => u.Role == UserRole.Student && u.StudentNo == "202110055");
    }
}
