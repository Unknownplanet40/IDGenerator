using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Security;
using IdGen.Core.Services;
using IdGen.Data;
using IdGen.Data.Repositories;
using IdGen.Data.Services;
using Xunit;

namespace IdGen.Tests;

public class EndToEndWorkflowTests
{
    private (
        AuthService Auth,
        AdminService Admin,
        StudentService Student,
        SettingsService Settings,
        NotificationService Notif,
        UserRepository UserRepo
    ) CreateAppServices(SqliteConnectionFactory factory)
    {
        var userRepo = new UserRepository(factory);
        var sectionRepo = new SectionRepository(factory);
        var settingRepo = new SettingRepository(factory);
        var subRepo = new IdSubmissionRepository(factory);
        var resetRepo = new ResetRequestRepository(factory);
        var notifRepo = new NotificationRepository(factory);

        var notifService = new NotificationService(notifRepo);
        var settingsService = new SettingsService(settingRepo);
        var authService = new AuthService(userRepo);
        var adminService = new AdminService(userRepo, sectionRepo, subRepo, resetRepo, notifService);
        var studentService = new StudentService(userRepo, subRepo, resetRepo);

        return (authService, adminService, studentService, settingsService, notifService, userRepo);
    }

    [Fact]
    public async Task FullLifecycle_AdminSetup_StudentRegister_Approve_SubmitId_Review_Reset_PasswordChange()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var (auth, admin, student, settings, notif, userRepo) = CreateAppServices(factory);

        // 1. First run: No admin exists
        bool hasAdminInitial = await auth.HasAnyAdminAsync();
        Assert.False(hasAdminInitial);

        // 2. Admin First-Time Setup
        var setupResult = await auth.CreateFirstAdminAsync("superadmin", "AdminSecret123!", "AdminSecret123!");
        Assert.True(setupResult.Success);
        Assert.True(await auth.HasAnyAdminAsync());

        // Setup admin again must fail
        var secondSetup = await auth.CreateFirstAdminAsync("otheradmin", "AdminSecret123!", "AdminSecret123!");
        Assert.False(secondSetup.Success);

        // 3. Admin Login
        var adminLogin = await auth.LoginAsync("superadmin", "AdminSecret123!");
        Assert.True(adminLogin.Success);
        Assert.NotNull(adminLogin.User);
        Assert.Equal(UserRole.Admin, adminLogin.User.Role);

        // 4. Admin Sets School Defaults
        await settings.SetSettingAsync("school_name", "National Science High School");
        await settings.SetSettingAsync("principal_name", "Dr. Jane C. Robles");

        string? schoolName = await settings.GetSettingAsync("school_name");
        string? principalName = await settings.GetSettingAsync("principal_name");
        Assert.Equal("National Science High School", schoolName);
        Assert.Equal("Dr. Jane C. Robles", principalName);

        // 5. Admin Creates Section
        var addedSec = await admin.AddSectionAsync("Grade 11", "Newton", "Prof. Albert Isaac");
        Assert.True(addedSec.Id > 0);

        var allSections = await admin.GetAllSectionsAsync();
        Assert.Single(allSections);
        Assert.Equal("Grade 11-Newton", allSections[0].DisplayName);

        // 6. Student Registers
        var regResult = await auth.RegisterStudentAsync(
            studentNo: "202110055",
            lastName: "Dela Cruz",
            firstName: "Juan",
            middleName: "M",
            email: "juan@school.edu",
            phone: "09171234567",
            username: "juandelacruz",
            password: "StudentPass123!",
            confirmPassword: "StudentPass123!");

        Assert.True(regResult.IsValid);

        var studentUser = await userRepo.GetByUsernameAsync("juandelacruz");
        Assert.NotNull(studentUser);
        int studentId = studentUser.Id;
        Assert.Equal(RegistrationStatus.Pending, studentUser.Status);

        // 7. Student Attempts Login Before Approval -> Must be blocked with A5 message
        var earlyLogin = await auth.LoginAsync("juandelacruz", "StudentPass123!");
        Assert.False(earlyLogin.Success);
        Assert.Equal("Your account is pending approval", earlyLogin.ErrorMessage);

        // 8. Admin Reviews and Approves Student
        var pendingRegs = await admin.GetPendingRegistrationsAsync();
        Assert.Single(pendingRegs);
        Assert.Equal("juandelacruz", pendingRegs[0].Username);

        await admin.ApproveRegistrationAsync(studentId);

        // Verify Student Notification
        var studentNotifs = await notif.GetUserNotificationsAsync(studentId);
        Assert.Single(studentNotifs);
        Assert.Contains("approved", studentNotifs[0].Message.ToLower());

        // 9. Student Logs In -> Success
        var studentLogin = await auth.LoginAsync("juandelacruz", "StudentPass123!");
        Assert.True(studentLogin.Success);
        Assert.NotNull(studentLogin.User);
        Assert.Equal(RegistrationStatus.Approved, studentLogin.User.Status);

        // 10. Student checks submission -> None exists yet
        var subBefore = await student.GetSubmissionAsync(studentId);
        Assert.Null(subBefore);

        // 11. Student Submits ID Form
        var submission = new IdSubmission
        {
            UserId = studentId,
            Gender = "Male",
            BirthDate = new DateOnly(2007, 3, 10),
            Address = "123 Academic Road, Metro Manila",
            GuardianName = "Maria Dela Cruz",
            GuardianPhone = "09181234567",
            SectionId = addedSec.Id,
            AdviserSnapshot = allSections[0].Adviser,
            PrincipalSnapshot = principalName!,
            SchoolSnapshot = schoolName!,
            SignaturePng = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 },
            Status = SubmissionStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        var createdSub = await student.SubmitIdAsync(submission);
        Assert.True(createdSub.Id > 0);
        Assert.Equal(SubmissionStatus.Pending, createdSub.Status);

        // Age calculation verified
        int age = createdSub.ComputeAge(new DateOnly(2025, 3, 10));
        Assert.Equal(18, age);

        // 12. Duplicate submission blocked
        await Assert.ThrowsAsync<InvalidOperationException>(() => student.SubmitIdAsync(submission));

        // 13. Admin Reviews Submissions and Approves
        var adminSubmissions = await admin.GetAllSubmissionsAsync();
        Assert.Single(adminSubmissions);
        Assert.Equal(studentId, adminSubmissions[0].UserId);

        await admin.ApproveSubmissionAsync(adminSubmissions[0].Id);

        // Student submission is now approved
        var approvedSub = await student.GetSubmissionAsync(studentId);
        Assert.NotNull(approvedSub);
        Assert.Equal(SubmissionStatus.Approved, approvedSub.Status);

        // 14. Section cannot be deleted while in use
        await Assert.ThrowsAsync<IdGen.Core.Exceptions.SectionInUseException>(() => admin.DeleteSectionAsync(addedSec.Id));

        // 15. Student Requests Reset
        var resetReq = await student.RequestResetAsync(studentId);
        Assert.NotNull(resetReq);
        Assert.Equal(ResetStatus.Pending, resetReq.Status);

        // Duplicate reset request blocked
        await Assert.ThrowsAsync<InvalidOperationException>(() => student.RequestResetAsync(studentId));

        // 16. Admin Approves Reset Request -> Submission deleted, unlocking generator
        var adminResetReqs = await admin.GetAllResetRequestsAsync();
        Assert.Single(adminResetReqs);
        Assert.Equal(resetReq.Id, adminResetReqs[0].Id);

        await admin.ApproveResetRequestAsync(resetReq.Id);

        // Submission is deleted
        var subAfterReset = await student.GetSubmissionAsync(studentId);
        Assert.Null(subAfterReset);

        // Pending reset request is resolved
        var pendingResetAfter = await student.GetPendingResetRequestAsync(studentId);
        Assert.Null(pendingResetAfter);

        // 17. Student Changes Password
        var wrongPassResult = await student.ChangePasswordAsync(studentId, "WrongOldPass", "BrandNewPass123!", "BrandNewPass123!");
        Assert.False(wrongPassResult.IsValid);

        var correctPassResult = await student.ChangePasswordAsync(studentId, "StudentPass123!", "BrandNewPass123!", "BrandNewPass123!");
        Assert.True(correctPassResult.IsValid);

        // Login with old password fails
        var oldLogin = await auth.LoginAsync("juandelacruz", "StudentPass123!");
        Assert.False(oldLogin.Success);
        Assert.Equal("Password is incorrect", oldLogin.ErrorMessage);

        // Login with new password succeeds
        var newLogin = await auth.LoginAsync("juandelacruz", "BrandNewPass123!");
        Assert.True(newLogin.Success);
    }
}
