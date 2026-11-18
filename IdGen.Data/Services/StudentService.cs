using IdGen.Core.Constants;
using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Security;
using IdGen.Core.Services;
using IdGen.Core.Validation;
using IdGen.Data.Repositories;

namespace IdGen.Data.Services;

public class StudentService : IStudentService
{
    private readonly IUserRepository _userRepository;
    private readonly IIdSubmissionRepository _submissionRepository;
    private readonly IResetRequestRepository _resetRequestRepository;
    private readonly ISectionRepository? _sectionRepository;
    private readonly ISettingsService? _settingsService;

    public StudentService(
        IUserRepository userRepository,
        IIdSubmissionRepository submissionRepository,
        IResetRequestRepository resetRequestRepository,
        ISectionRepository? sectionRepository = null,
        ISettingsService? settingsService = null)
    {
        _userRepository = userRepository;
        _submissionRepository = submissionRepository;
        _resetRequestRepository = resetRequestRepository;
        _sectionRepository = sectionRepository;
        _settingsService = settingsService;
    }

    public async Task<User?> GetStudentProfileAsync(int studentId)
    {
        return await _userRepository.GetByIdAsync(studentId);
    }

    public async Task<IdSubmission?> GetSubmissionAsync(int studentId)
    {
        return await _submissionRepository.GetByUserIdAsync(studentId);
    }

    public async Task<IdSubmission> SubmitIdAsync(IdSubmission submission)
    {
        var existing = await _submissionRepository.GetByUserIdAsync(submission.UserId);
        if (existing != null)
        {
            throw new InvalidOperationException("You already have an existing ID submission. Request an ID reset to resubmit.");
        }

        // Populate snapshots from section and settings if not already provided
        if (_sectionRepository != null && submission.SectionId > 0)
        {
            var section = await _sectionRepository.GetByIdAsync(submission.SectionId);
            if (section != null)
            {
                if (string.IsNullOrWhiteSpace(submission.AdviserSnapshot))
                    submission.AdviserSnapshot = section.Adviser;
                if (string.IsNullOrWhiteSpace(submission.CourseSnapshot))
                    submission.CourseSnapshot = section.Course;
            }
        }

        if (_settingsService != null)
        {
            if (string.IsNullOrWhiteSpace(submission.SchoolSnapshot))
                submission.SchoolSnapshot = await _settingsService.GetSettingAsync(SettingKeys.SchoolName) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(submission.PrincipalSnapshot))
                submission.PrincipalSnapshot = await _settingsService.GetSettingAsync(SettingKeys.PrincipalName) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(submission.CampusSnapshot))
                submission.CampusSnapshot = await _settingsService.GetSettingAsync(SettingKeys.CampusName);
            if (string.IsNullOrWhiteSpace(submission.PrincipalTitleSnapshot))
                submission.PrincipalTitleSnapshot = await _settingsService.GetSettingAsync(SettingKeys.PrincipalTitle);
            if (string.IsNullOrWhiteSpace(submission.SemesterSnapshot))
                submission.SemesterSnapshot = await _settingsService.GetSettingAsync(SettingKeys.Semester);
            if (string.IsNullOrWhiteSpace(submission.SchoolYearSnapshot))
                submission.SchoolYearSnapshot = await _settingsService.GetSettingAsync(SettingKeys.SchoolYear);
        }

        submission.Status = SubmissionStatus.Pending;
        submission.SubmittedAt = DateTime.UtcNow;

        int id = await _submissionRepository.CreateAsync(submission);
        submission.Id = id;
        return submission;
    }

    public async Task<ResetRequest?> GetPendingResetRequestAsync(int studentId)
    {
        return await _resetRequestRepository.GetPendingByUserIdAsync(studentId);
    }

    public async Task<ResetRequest> RequestResetAsync(int studentId)
    {
        var submission = await _submissionRepository.GetByUserIdAsync(studentId);
        if (submission == null)
        {
            throw new InvalidOperationException("You must have a submitted ID to request a reset.");
        }

        var existingPending = await _resetRequestRepository.GetPendingByUserIdAsync(studentId);
        if (existingPending != null)
        {
            throw new InvalidOperationException("You already have a pending ID reset request.");
        }

        var request = new ResetRequest
        {
            UserId = studentId,
            Status = ResetStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        int id = await _resetRequestRepository.CreateAsync(request);
        request.Id = id;
        return request;
    }

    public async Task<ValidationResult> ChangePasswordAsync(int studentId, string currentPassword, string newPassword, string confirmPassword)
    {
        var validation = InputValidators.ValidatePasswordChange(currentPassword, newPassword, confirmPassword);
        if (!validation.IsValid)
        {
            return validation;
        }

        var user = await _userRepository.GetByIdAsync(studentId);
        if (user == null)
        {
            return ValidationResult.Failed("currentPassword", "User not found.");
        }

        if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash, user.Salt, user.Iterations))
        {
            return ValidationResult.Failed("currentPassword", "Current password is incorrect.");
        }

        var (newHash, newSalt, newIterations) = PasswordHasher.HashPassword(newPassword);
        await _userRepository.UpdatePasswordAsync(studentId, newHash, newSalt, newIterations);

        return ValidationResult.Success();
    }
}
