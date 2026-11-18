using IdGen.Core.Validation;

namespace IdGen.Core.Services;

public interface IAuthService
{
    Task<bool> HasAnyAdminAsync();
    Task<AuthResult> CreateFirstAdminAsync(string username, string password, string confirmPassword);
    Task<AuthResult> LoginAsync(string username, string password);
    Task<ValidationResult> RegisterStudentAsync(
        string studentNo,
        string lastName,
        string firstName,
        string? middleName,
        string email,
        string phone,
        string username,
        string password,
        string confirmPassword);
}
