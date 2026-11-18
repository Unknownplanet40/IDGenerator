using IdGen.Core.Enums;
using IdGen.Core.Exceptions;
using IdGen.Core.Models;
using IdGen.Data.Repositories;
using Microsoft.Data.Sqlite;
using Xunit;

namespace IdGen.Tests;

public class SectionRepositoryTests
{
    [Fact]
    public async Task CreateAndGetSections_WorksCorrectly()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new SectionRepository(factory);

        var s1 = new Section { YearLevel = "Grade 11", SectionName = "STEM-A", Adviser = "Prof. Smith" };
        var s2 = new Section { YearLevel = "Grade 11", SectionName = "STEM-B", Adviser = "Prof. Jones" };

        await repo.CreateAsync(s1);
        await repo.CreateAsync(s2);

        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count);

        var found = await repo.GetByYearAndSectionAsync("Grade 11", "STEM-A");
        Assert.NotNull(found);
        Assert.Equal("Prof. Smith", found.Adviser);
    }

    [Fact]
    public async Task Create_DuplicateYearLevelAndSectionName_ThrowsSqliteException()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new SectionRepository(factory);

        var s1 = new Section { YearLevel = "Grade 12", SectionName = "ABM-A", Adviser = "Prof. Alpha" };
        var s2 = new Section { YearLevel = "Grade 12", SectionName = "ABM-A", Adviser = "Prof. Beta" };

        await repo.CreateAsync(s1);
        await Assert.ThrowsAsync<SqliteException>(() => repo.CreateAsync(s2));
    }

    [Fact]
    public async Task DeleteAsync_UnusedSection_DeletesSuccessfully()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new SectionRepository(factory);

        var s = new Section { YearLevel = "Grade 10", SectionName = "Rizal", Adviser = "Prof. Luna" };
        await repo.CreateAsync(s);

        await repo.DeleteAsync(s.Id);

        var check = await repo.GetByIdAsync(s.Id);
        Assert.Null(check);
    }

    [Fact]
    public async Task DeleteAsync_SectionInUseBySubmission_ThrowsSectionInUseException()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var userRepo = new UserRepository(factory);
        var sectionRepo = new SectionRepository(factory);
        var subRepo = new IdSubmissionRepository(factory);

        // Create Section
        var section = new Section { YearLevel = "Grade 11", SectionName = "HUMSS-1", Adviser = "Prof. Garcia" };
        await sectionRepo.CreateAsync(section);

        // Create User
        var student = new User
        {
            Role = UserRole.Student,
            Username = "studentX",
            PasswordHash = "hash",
            Salt = "salt",
            Iterations = 600_000,
            StudentNo = "2024-9999",
            LastName = "Reyes",
            FirstName = "Carlo",
            Email = "carlo@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Approved
        };
        await userRepo.CreateAsync(student);

        // Create Submission referencing this Section
        var submission = new IdSubmission
        {
            UserId = student.Id,
            Gender = "Male",
            BirthDate = new DateOnly(2006, 3, 20),
            Address = "123 Sample Ave",
            GuardianName = "Mario Reyes",
            GuardianPhone = "09181234567",
            SectionId = section.Id,
            AdviserSnapshot = "Prof. Garcia",
            PrincipalSnapshot = "Dr. Principal",
            SchoolSnapshot = "Sample University",
            SignaturePng = new byte[] { 1, 2, 3, 4 },
            Status = SubmissionStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };
        await subRepo.CreateAsync(submission);

        // Attempt to delete the section must throw SectionInUseException
        var ex = await Assert.ThrowsAsync<SectionInUseException>(() => sectionRepo.DeleteAsync(section.Id));
        Assert.Contains(section.Id.ToString(), ex.Message);

        // Verify section still exists
        var stillThere = await sectionRepo.GetByIdAsync(section.Id);
        Assert.NotNull(stillThere);
    }
}
