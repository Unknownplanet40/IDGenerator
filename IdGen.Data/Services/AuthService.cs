using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Security;
using IdGen.Core.Services;
using IdGen.Core.Validation;
using IdGen.Data.Repositories;

namespace IdGen.Data.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> HasAnyAdminAsync()
    {
        return await _userRepository.HasAnyAdminAsync();
    }

    public async Task<AuthResult> CreateFirstAdminAsync(string username, string password, string confirmPassword)
    {
        if (await _userRepository.HasAnyAdminAsync())
        {
            return AuthResult.Fail("An administrator account already exists.");
        }

        var validation = InputValidators.ValidateAdminSetup(username, password, confirmPassword);
        if (!validation.IsValid)
        {
            var firstError = validation.Errors.Values.FirstOrDefault() ?? "Invalid admin credentials.";
            return AuthResult.Fail(firstError);
        }

        var (hash, salt, iterations) = PasswordHasher.HashPassword(password);

        var admin = new User
        {
            Role = UserRole.Admin,
            Username = username.Trim(),
            PasswordHash = hash,
            Salt = salt,
            Iterations = iterations,
            LastName = "Administrator",
            FirstName = "System",
            Email = "admin@system.local",
            Phone = "09000000000",
            Status = RegistrationStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(admin);
        return AuthResult.Ok(admin);
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        bool usernameEmpty = string.IsNullOrWhiteSpace(username);
        bool passwordEmpty = string.IsNullOrWhiteSpace(password);

        if (usernameEmpty && passwordEmpty)
        {
            return AuthResult.Fail("Username and password are incorrect");
        }
        if (usernameEmpty)
        {
            return AuthResult.Fail("Username is incorrect");
        }
        if (passwordEmpty)
        {
            return AuthResult.Fail("Password is incorrect");
        }

        var user = await _userRepository.GetByUsernameAsync(username.Trim());
        if (user == null)
        {
            return AuthResult.Fail("Username is incorrect");
        }

        bool passwordMatches = PasswordHasher.VerifyPassword(password, user.PasswordHash, user.Salt, user.Iterations);
        if (!passwordMatches)
        {
            return AuthResult.Fail("Password is incorrect");
        }

        // Status gate for students
        if (user.Role == UserRole.Student)
        {
            if (user.Status == RegistrationStatus.Pending)
            {
                return AuthResult.Fail("Your account is pending approval");
            }
            if (user.Status == RegistrationStatus.Declined)
            {
                return AuthResult.Fail("Your registration was declined");
            }
        }

        return AuthResult.Ok(user);
    }

    public async Task<ValidationResult> RegisterStudentAsync(
        string studentNo,
        string lastName,
        string firstName,
        string? middleName,
        string email,
        string phone,
        string username,
        string password,
        string confirmPassword)
    {
        var validation = InputValidators.ValidateRegistration(
            studentNo, lastName, firstName, middleName, email, phone, username, password, confirmPassword);

        if (!validation.IsValid)
        {
            return validation;
        }

        // Check unique username
        var existingUser = await _userRepository.GetByUsernameAsync(username.Trim());
        if (existingUser != null)
        {
            validation.AddError(nameof(username), "Username is already taken.");
            return validation;
        }

        // Check unique student number
        string cleanStudentNo = studentNo.Trim().Replace("-", "");
        var existingStudentNo = await _userRepository.GetByStudentNoAsync(cleanStudentNo);
        if (existingStudentNo != null)
        {
            validation.AddError(nameof(studentNo), "Student ID number is already registered.");
            return validation;
        }

        var (hash, salt, iterations) = PasswordHasher.HashPassword(password);

        var student = new User
        {
            Role = UserRole.Student,
            StudentNo = cleanStudentNo,
            LastName = lastName.Trim(),
            FirstName = firstName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim(),
            Email = email.Trim(),
            Phone = phone.Trim().Replace(" ", "").Replace("-", ""),
            Username = username.Trim(),
            PasswordHash = hash,
            Salt = salt,
            Iterations = iterations,
            Status = RegistrationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(student);
        return validation;
    }
}
