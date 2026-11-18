using IdGen.Core.Models;

namespace IdGen.Core.Services;

public class AuthResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public User? User { get; init; }

    public static AuthResult Ok(User user) => new() { Success = true, User = user };
    public static AuthResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}
