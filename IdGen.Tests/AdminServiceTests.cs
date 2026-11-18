using IdGen.Core.Enums;
using IdGen.Core.Exceptions;
using IdGen.Core.Models;
using IdGen.Data;
using IdGen.Data.Repositories;
using IdGen.Data.Services;
using Xunit;

namespace IdGen.Tests;

public class AdminServiceTests
{
    private (AdminService AdminService, UserRepository UserRepo, SectionRepository SectionRepo, IdSubmissionRepository SubRepo, ResetRequestRepository ResetRepo, NotificationService NotifService) CreateService(SqliteConnectionFactory factory)
    {
        var userRepo = new UserRepository(factory);
        var sectionRepo = new SectionRepository(factory);
        var subRepo = new IdSubmissionRepository(factory);
        var resetRepo = new ResetRequestRepository(factory);
        var notifRepo = new NotificationRepository(factory);
        var notifService = new NotificationService(notifRepo);

        var adminService = new AdminService(userRepo, sectionRepo, subRepo, resetRepo, notifService);
        return (adminService, userRepo, sectionRepo, subRepo, resetRepo, notifService);
    }

    [Fact]
    public async Task Registrations_ApproveAndDecline_UpdatesStatusAndSendsNotification()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (adminService, userRepo, _, _, _, notifService) = CreateService(factory);

        var student1 = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-001",
            LastName = "One",
            FirstName = "Student",
            Email = "s1@school.edu",
            Phone = "09171234567",
            Username = "s1",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            Status = RegistrationStatus.Pending
        };
        await userRepo.CreateAsync(student1);

        var student2 = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-002",
            LastName = "Two",
            FirstName = "Student",
            Email = "s2@school.edu",
            Phone = "09171234568",
            Username = "s2",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            Status = RegistrationStatus.Pending
        };
        await userRepo.CreateAsync(student2);

        var pending = await adminService.GetPendingRegistrationsAsync();
        Assert.Equal(2, pending.Count);

        // Approve student 1
        await adminService.ApproveRegistrationAsync(student1.Id);
        var s1Check = await userRepo.GetByIdAsync(student1.Id);
        Assert.Equal(RegistrationStatus.Approved, s1Check!.Status);

        var s1Notifs = await notifService.GetUserNotificationsAsync(student1.Id);
        Assert.Single(s1Notifs);
        Assert.Contains("approved", s1Notifs[0].Message);

        // Decline student 2
        await adminService.DeclineRegistrationAsync(student2.Id);
        var s2Check = await userRepo.GetByIdAsync(student2.Id);
        Assert.Equal(RegistrationStatus.Declined, s2Check!.Status);

        var s2Notifs = await notifService.GetUserNotificationsAsync(student2.Id);
        Assert.Single(s2Notifs);
        Assert.Contains("declined", s2Notifs[0].Message);

        // No more pending
        var pendingAfter = await adminService.GetPendingRegistrationsAsync();
        Assert.Empty(pendingAfter);
    }

    [Fact]
    public async Task Sections_AddAndDelete_EnforcesDuplicateCheckAndInUseGuard()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (adminService, userRepo, sectionRepo, subRepo, _, _) = CreateService(factory);

        // Add section
        var sec = await adminService.AddSectionAsync("Grade 11", "STEM-A", "Prof. Ramos");
        Assert.True(sec.Id > 0);

        // Duplicate section name in same year throws
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            adminService.AddSectionAsync("Grade 11", "STEM-A", "Prof. Other"));

        // Delete unused section succeeds
        var sec2 = await adminService.AddSectionAsync("Grade 12", "HUMSS-B", "Prof. Santos");
        await adminService.DeleteSectionAsync(sec2.Id);
        Assert.Null(await sectionRepo.GetByIdAsync(sec2.Id));

        // Create student and submission referencing sec
        var student = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-003",
            LastName = "Three",
            FirstName = "Student",
            Email = "s3@school.edu",
            Phone = "09171234567",
            Username = "s3",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            Status = RegistrationStatus.Approved
        };
        await userRepo.CreateAsync(student);

        var submission = new IdSubmission
        {
            UserId = student.Id,
            Gender = "Female",
            BirthDate = new DateOnly(2005, 5, 5),
            Address = "Address",
            GuardianName = "Guardian",
            GuardianPhone = "09170000000",
            SectionId = sec.Id,
            AdviserSnapshot = "Prof. Ramos",
            PrincipalSnapshot = "Principal",
            SchoolSnapshot = "School",
            SignaturePng = new byte[] { 1, 2 },
            Status = SubmissionStatus.Pending
        };
        await subRepo.CreateAsync(submission);

        // Deleting sec must throw SectionInUseException (Task 4.2 / D2)
        await Assert.ThrowsAsync<SectionInUseException>(() => adminService.DeleteSectionAsync(sec.Id));
    }

    [Fact]
    public async Task Submissions_ApproveAndDisapprove_UpdatesStatusAndSendsNotification()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (adminService, userRepo, sectionRepo, subRepo, _, notifService) = CreateService(factory);

        var student = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-004",
            LastName = "Four",
            FirstName = "Student",
            Email = "s4@school.edu",
            Phone = "09171234567",
            Username = "s4",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            Status = RegistrationStatus.Approved
        };
        await userRepo.CreateAsync(student);

        int secId1 = await sectionRepo.CreateAsync(new Section { YearLevel = "Grade 11", SectionName = "A", Adviser = "Adv" });

        var submission = new IdSubmission
        {
            UserId = student.Id,
            Gender = "Male",
            BirthDate = new DateOnly(2005, 1, 1),
            Address = "Address",
            GuardianName = "Guardian",
            GuardianPhone = "09170000000",
            SectionId = secId1,
            AdviserSnapshot = "Adv",
            PrincipalSnapshot = "Prin",
            SchoolSnapshot = "Sch",
            SignaturePng = new byte[] { 1 },
            Status = SubmissionStatus.Pending
        };
        int subId = await subRepo.CreateAsync(submission);

        // Approve submission
        await adminService.ApproveSubmissionAsync(subId);
        var subApproved = await subRepo.GetByIdAsync(subId);
        Assert.Equal(SubmissionStatus.Approved, subApproved!.Status);

        var notifs = await notifService.GetUserNotificationsAsync(student.Id);
        Assert.Single(notifs);
        Assert.Contains("ready to claim", notifs[0].Message);

        // Disapprove submission
        await adminService.DisapproveSubmissionAsync(subId);
        var subDisapproved = await subRepo.GetByIdAsync(subId);
        Assert.Equal(SubmissionStatus.Disapproved, subDisapproved!.Status);

        var notifsAfter = await notifService.GetUserNotificationsAsync(student.Id);
        Assert.Equal(2, notifsAfter.Count);
        Assert.Contains("disapproved", notifsAfter[0].Message);
    }

    [Fact]
    public async Task ResetRequests_Approve_DeletesStudentSubmissionAndUnlocksGenerator()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (adminService, userRepo, sectionRepo, subRepo, resetRepo, notifService) = CreateService(factory);

        var student = new User
        {
            Role = UserRole.Student,
            StudentNo = "2024-005",
            LastName = "Five",
            FirstName = "Student",
            Email = "s5@school.edu",
            Phone = "09171234567",
            Username = "s5",
            PasswordHash = "h",
            Salt = "s",
            Iterations = 600_000,
            Status = RegistrationStatus.Approved
        };
        await userRepo.CreateAsync(student);

        int secId2 = await sectionRepo.CreateAsync(new Section { YearLevel = "Grade 11", SectionName = "B", Adviser = "Adv" });

        var submission = new IdSubmission
        {
            UserId = student.Id,
            Gender = "Male",
            BirthDate = new DateOnly(2005, 2, 2),
            Address = "Address",
            GuardianName = "Guardian",
            GuardianPhone = "09170000000",
            SectionId = secId2,
            AdviserSnapshot = "Adv",
            PrincipalSnapshot = "Prin",
            SchoolSnapshot = "Sch",
            SignaturePng = new byte[] { 1 },
            Status = SubmissionStatus.Pending
        };
        await subRepo.CreateAsync(submission);
        Assert.NotNull(await subRepo.GetByUserIdAsync(student.Id));

        var resetReq = new ResetRequest
        {
            UserId = student.Id,
            Status = ResetStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };
        int reqId = await resetRepo.CreateAsync(resetReq);

        // Approve reset request
        await adminService.ApproveResetRequestAsync(reqId);

        // Verify request is Approved
        var resolvedReq = await resetRepo.GetByIdAsync(reqId);
        Assert.Equal(ResetStatus.Approved, resolvedReq!.Status);
        Assert.NotNull(resolvedReq.ResolvedAt);

        // Spec D5: Approving reset request deletes student's submission to unlock generator!
        var subAfter = await subRepo.GetByUserIdAsync(student.Id);
        Assert.Null(subAfter);

        // Student receives notification
        var notifs = await notifService.GetUserNotificationsAsync(student.Id);
        Assert.Single(notifs);
        Assert.Contains("reset request has been approved", notifs[0].Message);
    }
}
