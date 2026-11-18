using IdGen.Core.Enums;
using IdGen.Core.Services;
using IdGen.Data.Repositories;
using IdGen.Data.Services;
using Xunit;

namespace IdGen.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task CreateFirstAdminAsync_CreatesAdmin_WhenNoAdminExists()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var authService = new AuthService(userRepo);

        Assert.False(await authService.HasAnyAdminAsync());

        var result = await authService.CreateFirstAdminAsync("superadmin", "Password123!", "Password123!");
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal(UserRole.Admin, result.User.Role);
        Assert.Equal(RegistrationStatus.Approved, result.User.Status);

        Assert.True(await authService.HasAnyAdminAsync());

        // Second attempt must fail
        var second = await authService.CreateFirstAdminAsync("secondadmin", "Password123!", "Password123!");
        Assert.False(second.Success);
        Assert.Equal("An administrator account already exists.", second.ErrorMessage);
    }

    [Fact]
    public async Task CreateFirstAdminAsync_PasswordTooShort_FailsValidation()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var authService = new AuthService(userRepo);

        var result = await authService.CreateFirstAdminAsync("admin", "short", "short");
        Assert.False(result.Success);
        Assert.Equal("Password must be at least 8 characters.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("", "", "Username and password are incorrect")]
    [InlineData("   ", "", "Username and password are incorrect")]
    [InlineData("", "password123", "Username is incorrect")]
    [InlineData("nonexistentUser", "", "Password is incorrect")]
    [InlineData("nonexistentUser", "somePass123", "Username is incorrect")]
    public async Task LoginAsync_ErrorCases_ReturnExactSpecMessages(string username, string password, string expectedError)
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var authService = new AuthService(userRepo);

        var result = await authService.LoginAsync(username, password);
        Assert.False(result.Success);
        Assert.Equal(expectedError, result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsPasswordIsIncorrect()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var authService = new AuthService(userRepo);

        await authService.CreateFirstAdminAsync("myadmin", "CorrectPassword123!", "CorrectPassword123!");

        var result = await authService.LoginAsync("myadmin", "WrongPassword123!");
        Assert.False(result.Success);
        Assert.Equal("Password is incorrect", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_PendingStudent_ReturnsAccountPendingApproval()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var authService = new AuthService(userRepo);

        await authService.RegisterStudentAsync(
            "202100001", "Dela Cruz", "Juan", null, "juan@school.edu", "09171234567",
            "juanStudent", "Password123!", "Password123!");

        var result = await authService.LoginAsync("juanStudent", "Password123!");
        Assert.False(result.Success);
        Assert.Equal("Your account is pending approval", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_DeclinedStudent_ReturnsRegistrationDeclined()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var authService = new AuthService(userRepo);

        await authService.RegisterStudentAsync(
            "202100002", "Santos", "Maria", null, "maria@school.edu", "09171234567",
            "mariaStudent", "Password123!", "Password123!");

        var user = await userRepo.GetByUsernameAsync("mariaStudent");
        Assert.NotNull(user);
        await userRepo.UpdateStatusAsync(user.Id, RegistrationStatus.Declined);

        var result = await authService.LoginAsync("mariaStudent", "Password123!");
        Assert.False(result.Success);
        Assert.Equal("Your registration was declined", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_ApprovedStudent_Succeeds()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var authService = new AuthService(userRepo);

        await authService.RegisterStudentAsync(
            "202100003", "Reyes", "Pedro", null, "pedro@school.edu", "09171234567",
            "pedroStudent", "Password123!", "Password123!");

        var user = await userRepo.GetByUsernameAsync("pedroStudent");
        Assert.NotNull(user);
        await userRepo.UpdateStatusAsync(user.Id, RegistrationStatus.Approved);

        var result = await authService.LoginAsync("pedroStudent", "Password123!");
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal(UserRole.Student, result.User.Role);
    }
}
