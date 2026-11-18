using IdGen.Core.Constants;
using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Data;
using IdGen.Data.Repositories;
using IdGen.Data.Services;
using Xunit;

namespace IdGen.Tests;

public class StepS2RepositoryAndSnapshotTests
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public StepS2RepositoryAndSnapshotTests()
    {
        _connectionFactory = SqliteTestFixture.CreateInMemoryFactory();
    }

    [Fact]
    public void SchemaMigration_Version2_AppliedSuccessfully()
    {
        using var conn = _connectionFactory.CreateConnection();
        int version = DatabaseMigrator.GetUserVersion(conn);
        Assert.Equal(DatabaseMigrator.CurrentSchemaVersion, version);
    }

    [Fact]
    public async Task SectionRepository_CompositeUniqueness_Enforced()
    {
        var repo = new SectionRepository(_connectionFactory);

        var sec1 = new Section
        {
            Course = "BSIT",
            YearLevel = "3",
            SectionName = "A",
            Adviser = "Prof. X"
        };
        int id1 = await repo.CreateAsync(sec1);
        Assert.True(id1 > 0);

        var dup = await repo.GetByCompositeKeyAsync("BSIT", "3", "A");
        Assert.NotNull(dup);
        Assert.Equal(id1, dup.Id);

        // Different course same year/section should be allowed
        var sec2 = new Section
        {
            Course = "BSCS",
            YearLevel = "3",
            SectionName = "A",
            Adviser = "Prof. Y"
        };
        int id2 = await repo.CreateAsync(sec2);
        Assert.True(id2 > 0);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task SectionRepository_SearchAndSort_Works()
    {
        var repo = new SectionRepository(_connectionFactory);

        await repo.CreateAsync(new Section { Course = "BSIT", YearLevel = "1", SectionName = "A", Adviser = "Adviser 1" });
        await repo.CreateAsync(new Section { Course = "BSCS", YearLevel = "2", SectionName = "B", Adviser = "Adviser 2" });
        await repo.CreateAsync(new Section { Course = "BSEMC", YearLevel = "3", SectionName = "C", Adviser = "UniqueAdviser" });

        var searchByCourse = await repo.SearchAsync("BSIT");
        Assert.Single(searchByCourse);

        var searchByAdviser = await repo.SearchAsync("UniqueAdviser");
        Assert.Single(searchByAdviser);

        var all = await repo.GetAllAsync();
        Assert.True(all.Count >= 3);
    }

    [Fact]
    public async Task SectionRepository_UpdateAsync_ModifiesRecord()
    {
        var repo = new SectionRepository(_connectionFactory);

        var sec = new Section { Course = "BSIT", YearLevel = "1", SectionName = "A", Adviser = "Old Adviser" };
        await repo.CreateAsync(sec);

        sec.Adviser = "New Adviser";
        sec.SectionName = "A-Updated";
        await repo.UpdateAsync(sec);

        var updated = await repo.GetByIdAsync(sec.Id);
        Assert.NotNull(updated);
        Assert.Equal("New Adviser", updated.Adviser);
        Assert.Equal("A-Updated", updated.SectionName);
    }

    [Fact]
    public async Task TemplateAssetRepository_SchoolLogo_ReplacesOldRow()
    {
        var repo = new TemplateAssetRepository(_connectionFactory);

        var logo1 = new TemplateAsset
        {
            Kind = "SchoolLogo",
            MimeType = "image/png",
            FileSize = 100,
            Data = new byte[] { 1, 2, 3 },
            CreatedAt = DateTime.UtcNow
        };
        int id1 = await repo.SaveAssetAsync(logo1);

        var logo2 = new TemplateAsset
        {
            Kind = "SchoolLogo",
            MimeType = "image/jpeg",
            FileSize = 200,
            Data = new byte[] { 4, 5, 6 },
            CreatedAt = DateTime.UtcNow
        };
        int id2 = await repo.SaveAssetAsync(logo2);

        var active = await repo.GetActiveAssetByKindAsync("SchoolLogo");
        Assert.NotNull(active);
        Assert.Equal(id2, active.Id);
        Assert.Equal("image/jpeg", active.MimeType);

        var old = await repo.GetByIdAsync(id1);
        Assert.Null(old); // Old was deleted
    }

    [Fact]
    public async Task TemplateRepository_SaveAndGetActive_Works()
    {
        var repo = new TemplateRepository(_connectionFactory);

        var template = new TemplateRecord
        {
            Orientation = "Portrait",
            FrontJson = "{\"background\":{\"color\":\"#FFFFFF\"},\"elements\":[]}",
            BackJson = "{\"background\":{\"color\":\"#000000\"},\"elements\":[]}"
        };

        int id = await repo.SaveAsync(template);
        Assert.True(id > 0);

        var loaded = await repo.GetActiveAsync();
        Assert.NotNull(loaded);
        Assert.Equal("Portrait", loaded.Orientation);
        Assert.Equal(template.FrontJson, loaded.FrontJson);

        // Update
        template.Orientation = "Landscape";
        await repo.SaveAsync(template);

        var updated = await repo.GetActiveAsync();
        Assert.NotNull(updated);
        Assert.Equal("Landscape", updated.Orientation);
    }

    [Fact]
    public async Task IdSubmission_SnapshotImmutability_MaintainedAfterSettingsChange()
    {
        var userRepo = new UserRepository(_connectionFactory);
        var secRepo = new SectionRepository(_connectionFactory);
        var subRepo = new IdSubmissionRepository(_connectionFactory);
        var resetRepo = new ResetRequestRepository(_connectionFactory);
        var settingsRepo = new SettingRepository(_connectionFactory);
        var notifRepo = new NotificationRepository(_connectionFactory);

        var settingsService = new SettingsService(settingsRepo);
        var studentService = new StudentService(userRepo, subRepo, resetRepo, secRepo, settingsService);

        // 1. Initial settings
        await settingsService.SetSettingAsync(SettingKeys.SchoolName, "Original School");
        await settingsService.SetSettingAsync(SettingKeys.PrincipalName, "Dr. Original Principal");
        await settingsService.SetSettingAsync(SettingKeys.CampusName, "Main Campus");
        await settingsService.SetSettingAsync(SettingKeys.PrincipalTitle, "Campus Director");
        await settingsService.SetSettingAsync(SettingKeys.Semester, "First");
        await settingsService.SetSettingAsync(SettingKeys.SchoolYear, "2024-2025");

        // 2. Create Section
        var sec = new Section
        {
            Course = "BSIT",
            YearLevel = "3",
            SectionName = "A",
            Adviser = "Prof. Original Adviser"
        };
        await secRepo.CreateAsync(sec);

        // 3. Create Student
        var student = new User
        {
            Role = UserRole.Student,
            Username = "student1",
            PasswordHash = "hash",
            Salt = "salt",
            Iterations = 600000,
            StudentNo = "202110055",
            LastName = "Doe",
            FirstName = "John",
            Email = "john@school.edu",
            Phone = "09171234567",
            Status = RegistrationStatus.Approved
        };
        await userRepo.CreateAsync(student);

        // 4. Submit ID
        var sub = new IdSubmission
        {
            UserId = student.Id,
            Gender = "Male",
            BirthDate = new DateOnly(2002, 5, 10),
            BloodType = "O+",
            Address = "123 Main St",
            GuardianName = "Jane Doe",
            GuardianPhone = "09177654321",
            SectionId = sec.Id,
            SignaturePng = new byte[] { 1, 2, 3 }
        };
        await studentService.SubmitIdAsync(sub);

        // 5. Verify snapshots captured
        var savedSub = await subRepo.GetByUserIdAsync(student.Id);
        Assert.NotNull(savedSub);
        Assert.Equal("Original School", savedSub.SchoolSnapshot);
        Assert.Equal("Dr. Original Principal", savedSub.PrincipalSnapshot);
        Assert.Equal("Prof. Original Adviser", savedSub.AdviserSnapshot);
        Assert.Equal("BSIT", savedSub.CourseSnapshot);
        Assert.Equal("Main Campus", savedSub.CampusSnapshot);
        Assert.Equal("Campus Director", savedSub.PrincipalTitleSnapshot);
        Assert.Equal("First", savedSub.SemesterSnapshot);
        Assert.Equal("2024-2025", savedSub.SchoolYearSnapshot);
        Assert.Equal("O+", savedSub.BloodType);

        // 6. Admin modifies ALL settings and section details later
        await settingsService.SetSettingAsync(SettingKeys.SchoolName, "Brand New University");
        await settingsService.SetSettingAsync(SettingKeys.PrincipalName, "Dr. New Principal");
        await settingsService.SetSettingAsync(SettingKeys.CampusName, "North Campus");
        await settingsService.SetSettingAsync(SettingKeys.PrincipalTitle, "University President");
        await settingsService.SetSettingAsync(SettingKeys.Semester, "Second");
        await settingsService.SetSettingAsync(SettingKeys.SchoolYear, "2025-2026");

        sec.Adviser = "Prof. Changed Adviser";
        sec.Course = "BSCS";
        await secRepo.UpdateAsync(sec);

        // 7. Verify issued ID snapshots are 100% UNCHANGED
        var reloadedSub = await subRepo.GetByUserIdAsync(student.Id);
        Assert.NotNull(reloadedSub);
        Assert.Equal("Original School", reloadedSub.SchoolSnapshot);
        Assert.Equal("Dr. Original Principal", reloadedSub.PrincipalSnapshot);
        Assert.Equal("Prof. Original Adviser", reloadedSub.AdviserSnapshot);
        Assert.Equal("BSIT", reloadedSub.CourseSnapshot);
        Assert.Equal("Main Campus", reloadedSub.CampusSnapshot);
        Assert.Equal("Campus Director", reloadedSub.PrincipalTitleSnapshot);
        Assert.Equal("First", reloadedSub.SemesterSnapshot);
        Assert.Equal("2024-2025", reloadedSub.SchoolYearSnapshot);
        Assert.Equal("O+", reloadedSub.BloodType);
    }
}
