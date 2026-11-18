using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Security;
using IdGen.Data;
using IdGen.Data.Repositories;
using IdGen.Data.Services;
using Xunit;

namespace IdGen.Tests;

public class StudentServiceTests
{
    private (StudentService StudentService, UserRepository UserRepo, SectionRepository SectionRepo, IdSubmissionRepository SubRepo, ResetRequestRepository ResetRepo) CreateService(SqliteConnectionFactory factory)
    {
        var userRepo = new UserRepository(factory);
        var sectionRepo = new SectionRepository(factory);
        var subRepo = new IdSubmissionRepository(factory);
        var resetRepo = new ResetRequestRepository(factory);

        var studentService = new StudentService(userRepo, subRepo, resetRepo);
        return (studentService, userRepo, sectionRepo, subRepo, resetRepo);
    }

    [Fact]
    public async Task GetStudentProfileAsync_ReturnsUserProfile()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (studentService, userRepo, _, _, _) = CreateService(factory);

        var user = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-5555",
            LastName = "Doe",
            FirstName = "Jane",
            MiddleName = "A",
            Email = "jane@school.edu",
            Phone = "09171234567",
            Username = "janedoe",
            PasswordHash = "hash",
            Salt = "salt",
            Iterations = 600_000,
            Status = RegistrationStatus.Approved
        };
        int id = await userRepo.CreateAsync(user);

        var profile = await studentService.GetStudentProfileAsync(id);
        Assert.NotNull(profile);
        Assert.Equal("2024-5555", profile.StudentNo);
        Assert.Equal("Doe", profile.LastName);
        Assert.Equal("Jane", profile.FirstName);
    }

    [Fact]
    public async Task SubmitIdAsync_Success_SetsPendingAndPersists()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (studentService, userRepo, sectionRepo, _, _) = CreateService(factory);

        var user = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-1001",
            LastName = "Smith",
            FirstName = "John",
            Email = "john@school.edu",
            Phone = "09181234567",
            Username = "johnsmith",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            Status = RegistrationStatus.Approved
        };
        int userId = await userRepo.CreateAsync(user);

        var section = new Section { YearLevel = "Grade 11", SectionName = "Einstein", Adviser = "Prof. Newton" };
        int sectionId = await sectionRepo.CreateAsync(section);

        var submission = new IdSubmission
        {
            UserId = userId,
            Gender = "Male",
            BirthDate = new DateOnly(2006, 5, 20),
            Address = "123 Academic St.",
            GuardianName = "Mary Smith",
            GuardianPhone = "09187654321",
            SectionId = sectionId,
            AdviserSnapshot = "Prof. Newton",
            PrincipalSnapshot = "Dr. Principal",
            SchoolSnapshot = "Apex University",
            SignaturePng = new byte[] { 1, 2, 3 }
        };

        var created = await studentService.SubmitIdAsync(submission);
        Assert.True(created.Id > 0);
        Assert.Equal(SubmissionStatus.Pending, created.Status);

        var fetched = await studentService.GetSubmissionAsync(userId);
        Assert.NotNull(fetched);
        Assert.Equal("123 Academic St.", fetched.Address);
    }

    [Fact]
    public async Task SubmitIdAsync_DuplicateSubmission_ThrowsInvalidOperationException()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (studentService, userRepo, sectionRepo, _, _) = CreateService(factory);

        var user = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-1002",
            LastName = "Duplicate",
            FirstName = "User",
            Email = "dup@school.edu",
            Phone = "09181234567",
            Username = "dupuser",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            Status = RegistrationStatus.Approved
        };
        int userId = await userRepo.CreateAsync(user);

        var section = new Section { YearLevel = "Grade 11", SectionName = "Franklin", Adviser = "Prof. Ben" };
        int sectionId = await sectionRepo.CreateAsync(section);

        var sub1 = new IdSubmission
        {
            UserId = userId,
            Gender = "Female",
            BirthDate = new DateOnly(2005, 1, 1),
            Address = "Addr",
            GuardianName = "Guard",
            GuardianPhone = "09191234567",
            SectionId = sectionId,
            AdviserSnapshot = "Prof. Ben",
            PrincipalSnapshot = "Dr. P",
            SchoolSnapshot = "Sch",
            SignaturePng = new byte[] { 9, 9 }
        };
        await studentService.SubmitIdAsync(sub1);

        var sub2 = new IdSubmission
        {
            UserId = userId,
            Gender = "Female",
            BirthDate = new DateOnly(2005, 1, 1),
            Address = "Addr 2",
            GuardianName = "Guard",
            GuardianPhone = "09191234567",
            SectionId = sectionId,
            AdviserSnapshot = "Prof. Ben",
            PrincipalSnapshot = "Dr. P",
            SchoolSnapshot = "Sch",
            SignaturePng = new byte[] { 9, 9 }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => studentService.SubmitIdAsync(sub2));
    }

    [Fact]
    public async Task RequestResetAsync_WithoutSubmission_ThrowsInvalidOperationException()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (studentService, userRepo, _, _, _) = CreateService(factory);

        var user = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-1003",
            LastName = "NoSub",
            FirstName = "User",
            Email = "nosub@school.edu",
            Phone = "09181234567",
            Username = "nosub",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            Status = RegistrationStatus.Approved
        };
        int userId = await userRepo.CreateAsync(user);

        await Assert.ThrowsAsync<InvalidOperationException>(() => studentService.RequestResetAsync(userId));
    }

    [Fact]
    public async Task RequestResetAsync_Success_AndDuplicateThrows()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (studentService, userRepo, sectionRepo, _, _) = CreateService(factory);

        var user = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-1004",
            LastName = "Reset",
            FirstName = "Tester",
            Email = "reset@school.edu",
            Phone = "09181234567",
            Username = "resettester",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            Status = RegistrationStatus.Approved
        };
        int userId = await userRepo.CreateAsync(user);

        var section = new Section { YearLevel = "Grade 12", SectionName = "Tesla", Adviser = "Prof. Nikola" };
        int sectionId = await sectionRepo.CreateAsync(section);

        var submission = new IdSubmission
        {
            UserId = userId,
            Gender = "Male",
            BirthDate = new DateOnly(2004, 3, 15),
            Address = "Address",
            GuardianName = "Parent",
            GuardianPhone = "09181234567",
            SectionId = sectionId,
            AdviserSnapshot = "Prof. Nikola",
            PrincipalSnapshot = "Principal",
            SchoolSnapshot = "School",
            SignaturePng = new byte[] { 1 }
        };
        await studentService.SubmitIdAsync(submission);

        var req = await studentService.RequestResetAsync(userId);
        Assert.NotNull(req);
        Assert.Equal(ResetStatus.Pending, req.Status);

        var pending = await studentService.GetPendingResetRequestAsync(userId);
        Assert.NotNull(pending);
        Assert.Equal(req.Id, pending.Id);

        // Attempting to request again while one is pending must throw
        await Assert.ThrowsAsync<InvalidOperationException>(() => studentService.RequestResetAsync(userId));
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ReturnsError()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (studentService, userRepo, _, _, _) = CreateService(factory);

        var (hash, salt, iterations) = PasswordHasher.HashPassword("CorrectOldPass123");

        var user = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-1005",
            LastName = "Security",
            FirstName = "User",
            Email = "sec@school.edu",
            Phone = "09181234567",
            Username = "secuser",
            PasswordHash = hash,
            Salt = salt,
            Iterations = iterations,
            Status = RegistrationStatus.Approved
        };
        int userId = await userRepo.CreateAsync(user);

        var result = await studentService.ChangePasswordAsync(userId, "WrongPass123", "NewSecretPass123", "NewSecretPass123");
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("currentPassword"));
        Assert.Equal("Current password is incorrect.", result.Errors["currentPassword"]);
    }

    [Fact]
    public async Task ChangePasswordAsync_Success_UpdatesPasswordHash()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (studentService, userRepo, _, _, _) = CreateService(factory);

        var (hash, salt, iterations) = PasswordHasher.HashPassword("InitialPass123");

        var user = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-1006",
            LastName = "Security",
            FirstName = "User2",
            Email = "sec2@school.edu",
            Phone = "09181234567",
            Username = "secuser2",
            PasswordHash = hash,
            Salt = salt,
            Iterations = iterations,
            Status = RegistrationStatus.Approved
        };
        int userId = await userRepo.CreateAsync(user);

        var result = await studentService.ChangePasswordAsync(userId, "InitialPass123", "UpdatedPass456", "UpdatedPass456");
        Assert.True(result.IsValid);

        var updated = await userRepo.GetByIdAsync(userId);
        Assert.NotNull(updated);
        Assert.NotEqual(hash, updated.PasswordHash);
        Assert.NotEqual(salt, updated.Salt);

        // Verify that the new password works and the old one fails
        Assert.True(PasswordHasher.VerifyPassword("UpdatedPass456", updated.PasswordHash, updated.Salt, updated.Iterations));
        Assert.False(PasswordHasher.VerifyPassword("InitialPass123", updated.PasswordHash, updated.Salt, updated.Iterations));
    }
}
