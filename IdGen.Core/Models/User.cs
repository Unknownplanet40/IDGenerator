using IdGen.Core.Enums;

namespace IdGen.Core.Models;

public class User
{
    public int Id { get; set; }
    public UserRole Role { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public int Iterations { get; set; }
    public string? StudentNo { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {MiddleName} {LastName}";

    public string StudentNoDisplay => string.IsNullOrWhiteSpace(StudentNo) ? "—" : StudentNo;
    public string CreatedAtDisplay => CreatedAt.ToString("yyyy-MM-dd HH:mm");
    public string RoleDisplay => Role.ToString();
    public string StatusDisplay => Status.ToString();
    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsStudent => Role == UserRole.Student;
}
