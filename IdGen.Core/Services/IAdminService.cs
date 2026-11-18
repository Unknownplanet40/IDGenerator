using IdGen.Core.Models;

namespace IdGen.Core.Services;

public interface IAdminService
{
    Task<IReadOnlyList<User>> GetPendingRegistrationsAsync();
    Task<IReadOnlyList<User>> GetAllUsersAsync();
    Task ApproveRegistrationAsync(int userId);
    Task DeclineRegistrationAsync(int userId);

    Task<IReadOnlyList<Section>> GetAllSectionsAsync();
    Task<IReadOnlyList<Section>> SearchSectionsAsync(string? query);
    Task<Section> AddSectionAsync(string yearLevel, string sectionName, string adviser, string? course = null);
    Task<Section> UpdateSectionAsync(int id, string yearLevel, string sectionName, string adviser, string? course = null);
    Task DeleteSectionAsync(int sectionId);

    Task<IReadOnlyList<IdSubmission>> GetAllSubmissionsAsync();
    Task ApproveSubmissionAsync(int submissionId);
    Task DisapproveSubmissionAsync(int submissionId);

    Task<IReadOnlyList<ResetRequest>> GetAllResetRequestsAsync();
    Task<IReadOnlyList<ResetRequest>> GetPendingResetRequestsAsync();
    Task ApproveResetRequestAsync(int resetRequestId);
    Task DeclineResetRequestAsync(int resetRequestId);
}
