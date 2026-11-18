using IdGen.Core.Models;
using IdGen.Core.Validation;

namespace IdGen.Core.Services;

public interface IStudentService
{
    Task<User?> GetStudentProfileAsync(int studentId);
    Task<IdSubmission?> GetSubmissionAsync(int studentId);
    Task<IdSubmission> SubmitIdAsync(IdSubmission submission);
    Task<ResetRequest?> GetPendingResetRequestAsync(int studentId);
    Task<ResetRequest> RequestResetAsync(int studentId);
    Task<ValidationResult> ChangePasswordAsync(int studentId, string currentPassword, string newPassword, string confirmPassword);
}
