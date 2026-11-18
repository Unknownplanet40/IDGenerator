using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Services;
using IdGen.Core.Validation;
using IdGen.Data.Repositories;

namespace IdGen.Data.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly ISectionRepository _sectionRepository;
    private readonly IIdSubmissionRepository _submissionRepository;
    private readonly IResetRequestRepository _resetRequestRepository;
    private readonly INotificationService _notificationService;

    public AdminService(
        IUserRepository userRepository,
        ISectionRepository sectionRepository,
        IIdSubmissionRepository submissionRepository,
        IResetRequestRepository resetRequestRepository,
        INotificationService notificationService)
    {
        _userRepository = userRepository;
        _sectionRepository = sectionRepository;
        _submissionRepository = submissionRepository;
        _resetRequestRepository = resetRequestRepository;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<User>> GetPendingRegistrationsAsync()
    {
        return await _userRepository.GetPendingStudentsAsync();
    }

    public async Task<IReadOnlyList<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task ApproveRegistrationAsync(int userId)
    {
        await _userRepository.UpdateStatusAsync(userId, RegistrationStatus.Approved);
        await _notificationService.SendNotificationAsync(userId, "Your student account registration has been approved! You can now log in and generate your ID.");
    }

    public async Task DeclineRegistrationAsync(int userId)
    {
        await _userRepository.UpdateStatusAsync(userId, RegistrationStatus.Declined);
        await _notificationService.SendNotificationAsync(userId, "Your student account registration was declined. Please contact the administrator.");
    }

    public async Task<IReadOnlyList<Section>> GetAllSectionsAsync()
    {
        return await _sectionRepository.GetAllAsync();
    }

    public async Task<IReadOnlyList<Section>> SearchSectionsAsync(string? query)
    {
        return await _sectionRepository.SearchAsync(query);
    }

    public async Task<Section> AddSectionAsync(string yearLevel, string sectionName, string adviser, string? course = null)
    {
        string? cleanCourse = string.IsNullOrWhiteSpace(course) ? null : course.Trim();
        var validation = InputValidators.ValidateSection(yearLevel, sectionName, adviser, cleanCourse);
        if (!validation.IsValid)
        {
            var msg = validation.Errors.Values.FirstOrDefault() ?? "Invalid section parameters.";
            throw new ArgumentException(msg);
        }

        var existing = await _sectionRepository.GetByCompositeKeyAsync(cleanCourse, yearLevel.Trim(), sectionName.Trim());
        if (existing != null)
        {
            string label = !string.IsNullOrWhiteSpace(cleanCourse) ? $"{cleanCourse} {yearLevel.Trim()}-{sectionName.Trim()}" : $"{yearLevel.Trim()}-{sectionName.Trim()}";
            throw new InvalidOperationException($"Section '{label}' already exists.");
        }

        var section = new Section
        {
            Course = cleanCourse,
            YearLevel = yearLevel.Trim(),
            SectionName = sectionName.Trim(),
            Adviser = adviser.Trim()
        };

        await _sectionRepository.CreateAsync(section);
        return section;
    }

    public async Task<Section> UpdateSectionAsync(int id, string yearLevel, string sectionName, string adviser, string? course = null)
    {
        var section = await _sectionRepository.GetByIdAsync(id);
        if (section == null)
        {
            throw new KeyNotFoundException($"Section with ID {id} not found.");
        }

        string? cleanCourse = string.IsNullOrWhiteSpace(course) ? null : course.Trim();
        var validation = InputValidators.ValidateSection(yearLevel, sectionName, adviser, cleanCourse);
        if (!validation.IsValid)
        {
            var msg = validation.Errors.Values.FirstOrDefault() ?? "Invalid section parameters.";
            throw new ArgumentException(msg);
        }

        var existing = await _sectionRepository.GetByCompositeKeyAsync(cleanCourse, yearLevel.Trim(), sectionName.Trim(), excludeId: id);
        if (existing != null)
        {
            string label = !string.IsNullOrWhiteSpace(cleanCourse) ? $"{cleanCourse} {yearLevel.Trim()}-{sectionName.Trim()}" : $"{yearLevel.Trim()}-{sectionName.Trim()}";
            throw new InvalidOperationException($"Section '{label}' already exists.");
        }

        section.Course = cleanCourse;
        section.YearLevel = yearLevel.Trim();
        section.SectionName = sectionName.Trim();
        section.Adviser = adviser.Trim();

        await _sectionRepository.UpdateAsync(section);
        return section;
    }

    public async Task DeleteSectionAsync(int sectionId)
    {
        await _sectionRepository.DeleteAsync(sectionId);
    }

    public async Task<IReadOnlyList<IdSubmission>> GetAllSubmissionsAsync()
    {
        return await _submissionRepository.GetAllAsync();
    }

    public async Task ApproveSubmissionAsync(int submissionId)
    {
        var sub = await _submissionRepository.GetByIdAsync(submissionId);
        if (sub == null) return;

        await _submissionRepository.UpdateStatusAsync(submissionId, SubmissionStatus.Approved);
        await _notificationService.SendNotificationAsync(sub.UserId, "Your student ID submission has been approved! Your ID is ready to claim.");
    }

    public async Task DisapproveSubmissionAsync(int submissionId)
    {
        var sub = await _submissionRepository.GetByIdAsync(submissionId);
        if (sub == null) return;

        await _submissionRepository.UpdateStatusAsync(submissionId, SubmissionStatus.Disapproved);
        await _notificationService.SendNotificationAsync(sub.UserId, "Your student ID submission was disapproved. Please coordinate with the administrator.");
    }

    public async Task<IReadOnlyList<ResetRequest>> GetAllResetRequestsAsync()
    {
        return await _resetRequestRepository.GetAllAsync();
    }

    public async Task<IReadOnlyList<ResetRequest>> GetPendingResetRequestsAsync()
    {
        return await _resetRequestRepository.GetPendingAsync();
    }

    public async Task ApproveResetRequestAsync(int resetRequestId)
    {
        var request = await _resetRequestRepository.GetByIdAsync(resetRequestId);
        if (request == null) return;

        await _resetRequestRepository.UpdateStatusAsync(resetRequestId, ResetStatus.Approved, DateTime.UtcNow);

        // Delete that student's submission to unlock the ID generator
        await _submissionRepository.DeleteByUserIdAsync(request.UserId);

        await _notificationService.SendNotificationAsync(request.UserId, "Your ID reset request has been approved. You may now generate and submit a new ID form.");
    }

    public async Task DeclineResetRequestAsync(int resetRequestId)
    {
        var request = await _resetRequestRepository.GetByIdAsync(resetRequestId);
        if (request == null) return;

        await _resetRequestRepository.UpdateStatusAsync(resetRequestId, ResetStatus.Declined, DateTime.UtcNow);
        await _notificationService.SendNotificationAsync(request.UserId, "Your ID reset request was declined. Please coordinate with the administrator.");
    }
}
