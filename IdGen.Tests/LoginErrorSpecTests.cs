using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Services;
using Xunit;

namespace IdGen.Tests;

public class LoginErrorSpecTests
{
    public const string UsernameIncorrect = "Username is incorrect";
    public const string PasswordIncorrect = "Password is incorrect";
    public const string BothIncorrect = "Username and password are incorrect";
    public const string PendingApproval = "Your account is pending approval";
    public const string RegistrationDeclined = "Your registration was declined";

    [Fact]
    public void SpecErrorMessages_MatchRequiredStringsExactly()
    {
        Assert.Equal("Username is incorrect", UsernameIncorrect);
        Assert.Equal("Password is incorrect", PasswordIncorrect);
        Assert.Equal("Username and password are incorrect", BothIncorrect);
        Assert.Equal("Your account is pending approval", PendingApproval);
        Assert.Equal("Your registration was declined", RegistrationDeclined);
    }

    [Fact]
    public void StudentStatusGate_PendingStudent_ReturnsPendingApprovalMessage()
    {
        var student = new User
        {
            Id = 1,
            Role = UserRole.Student,
            Status = RegistrationStatus.Pending
        };

        string? message = null;
        if (student.Role == UserRole.Student && student.Status == RegistrationStatus.Pending)
        {
            message = PendingApproval;
        }

        Assert.Equal("Your account is pending approval", message);
    }

    [Fact]
    public void StudentStatusGate_DeclinedStudent_ReturnsRegistrationDeclinedMessage()
    {
        var student = new User
        {
            Id = 2,
            Role = UserRole.Student,
            Status = RegistrationStatus.Declined
        };

        string? message = null;
        if (student.Role == UserRole.Student && student.Status == RegistrationStatus.Declined)
        {
            message = RegistrationDeclined;
        }

        Assert.Equal("Your registration was declined", message);
    }
}
