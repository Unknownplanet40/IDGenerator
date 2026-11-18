using IdGen.Core.Enums;
using IdGen.Core.Models;

namespace IdGen.App.Services;

public interface IUserSession
{
    User? CurrentUser { get; }
    void SetUser(User user);
    void Clear();
    bool IsAdmin { get; }
    bool IsStudent { get; }
}

public class UserSession : IUserSession
{
    public User? CurrentUser { get; private set; }

    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
    public bool IsStudent => CurrentUser?.Role == UserRole.Student;

    public void SetUser(User user)
    {
        CurrentUser = user;
    }

    public void Clear()
    {
        CurrentUser = null;
    }
}
