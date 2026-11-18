using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Data.Repositories;
using Microsoft.Data.Sqlite;
using Xunit;

namespace IdGen.Tests;

public class IdSubmissionRepositoryTests
{
    [Fact]
    public async Task CreateAndRetrieve_IdSubmission_PreservesBlobAndFields()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var sectionRepo = new SectionRepository(factory);
        var repo = new IdSubmissionRepository(factory);

        var user = new User
        {
            Role = UserRole.Student,
            Username = "student1",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            StudentNo = "2024-0010",
            LastName = "Doe",
            FirstName = "Jane",
            Email = "jane@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Approved
        };
        await userRepo.CreateAsync(user);

        var section = new Section { YearLevel = "Grade 11", SectionName = "STEM-1", Adviser = "Prof. X" };
        await sectionRepo.CreateAsync(section);

        byte[] fakePng = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        var submission = new IdSubmission
        {
            UserId = user.Id,
            Gender = "Female",
            BirthDate = new DateOnly(2005, 8, 25),
            Address = "Barangay San Jose, Cavite",
            GuardianName = "Mary Doe",
            GuardianPhone = "09191234567",
            SectionId = section.Id,
            AdviserSnapshot = "Prof. X",
            PrincipalSnapshot = "Dr. Principal",
            SchoolSnapshot = "Test High",
            SignaturePng = fakePng,
            Status = SubmissionStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        int id = await repo.CreateAsync(submission);
        Assert.True(id > 0);

        var retrieved = await repo.GetByUserIdAsync(user.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Female", retrieved.Gender);
        Assert.Equal(new DateOnly(2005, 8, 25), retrieved.BirthDate);
        Assert.Equal(fakePng, retrieved.SignaturePng);
        Assert.Equal(SubmissionStatus.Pending, retrieved.Status);

        // Update status to Approved
        await repo.UpdateStatusAsync(id, SubmissionStatus.Approved);
        var updated = await repo.GetByIdAsync(id);
        Assert.NotNull(updated);
        Assert.Equal(SubmissionStatus.Approved, updated.Status);
    }

    [Fact]
    public async Task Create_DuplicateSubmissionForSameUser_ThrowsSqliteException()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var sectionRepo = new SectionRepository(factory);
        var repo = new IdSubmissionRepository(factory);

        var user = new User
        {
            Role = UserRole.Student,
            Username = "studentDouble",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            StudentNo = "2024-9988",
            LastName = "Doe",
            FirstName = "John",
            Email = "john@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Approved
        };
        await userRepo.CreateAsync(user);

        var section = new Section { YearLevel = "Grade 11", SectionName = "GAS-1", Adviser = "Prof. Y" };
        await sectionRepo.CreateAsync(section);

        var sub1 = new IdSubmission
        {
            UserId = user.Id,
            Gender = "Male",
            BirthDate = new DateOnly(2006, 1, 1),
            Address = "Cavite",
            GuardianName = "Parent",
            GuardianPhone = "09170000000",
            SectionId = section.Id,
            AdviserSnapshot = "Prof. Y",
            PrincipalSnapshot = "Principal",
            SchoolSnapshot = "School",
            SignaturePng = new byte[] { 1 },
            Status = SubmissionStatus.Pending
        };
        await repo.CreateAsync(sub1);

        var sub2 = new IdSubmission
        {
            UserId = user.Id,
            Gender = "Male",
            BirthDate = new DateOnly(2006, 1, 1),
            Address = "Cavite",
            GuardianName = "Parent",
            GuardianPhone = "09170000000",
            SectionId = section.Id,
            AdviserSnapshot = "Prof. Y",
            PrincipalSnapshot = "Principal",
            SchoolSnapshot = "School",
            SignaturePng = new byte[] { 1 },
            Status = SubmissionStatus.Pending
        };

        // User can only have one active submission
        await Assert.ThrowsAsync<SqliteException>(() => repo.CreateAsync(sub2));
    }

    [Fact]
    public async Task DeleteByUserIdAsync_ClearsSubmissionToAllowResubmission()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var sectionRepo = new SectionRepository(factory);
        var repo = new IdSubmissionRepository(factory);

        var user = new User
        {
            Role = UserRole.Student,
            Username = "studentReset",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            StudentNo = "2024-5555",
            LastName = "Reset",
            FirstName = "Student",
            Email = "reset@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Approved
        };
        await userRepo.CreateAsync(user);

        var section = new Section { YearLevel = "Grade 11", SectionName = "TVL-1", Adviser = "Prof. Z" };
        await sectionRepo.CreateAsync(section);

        var sub = new IdSubmission
        {
            UserId = user.Id,
            Gender = "Male",
            BirthDate = new DateOnly(2006, 2, 2),
            Address = "Cavite",
            GuardianName = "Parent",
            GuardianPhone = "09170000000",
            SectionId = section.Id,
            AdviserSnapshot = "Prof. Z",
            PrincipalSnapshot = "Principal",
            SchoolSnapshot = "School",
            SignaturePng = new byte[] { 1 },
            Status = SubmissionStatus.Pending
        };
        await repo.CreateAsync(sub);

        Assert.NotNull(await repo.GetByUserIdAsync(user.Id));

        // When reset is approved by admin, submission is deleted
        await repo.DeleteByUserIdAsync(user.Id);

        Assert.Null(await repo.GetByUserIdAsync(user.Id));

        // Now resubmission is allowed without unique constraint violation
        int newId = await repo.CreateAsync(sub);
        Assert.True(newId > 0);
    }
}
