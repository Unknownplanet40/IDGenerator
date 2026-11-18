using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.App.Services;
using IdGen.Core.Services;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.ViewModels.Student;

public partial class StudentAccountViewModel : ObservableObject
{
    private readonly IStudentService _studentService;
    private readonly IAdminService _adminService;
    private readonly IUserSession _userSession;

    [ObservableProperty]
    private bool _isLoading = true;

    // Registration Profile Info (Read-only)
    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _studentNo = string.Empty;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _accountStatus = string.Empty;

    // ID Data Profile Info (Read-only, blank if not yet submitted)
    [ObservableProperty]
    private string _gender = string.Empty;

    [ObservableProperty]
    private string _birthDate = string.Empty;

    [ObservableProperty]
    private string _age = string.Empty;

    [ObservableProperty]
    private string _yearAndSection = string.Empty;

    [ObservableProperty]
    private string _adviser = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _guardianName = string.Empty;

    [ObservableProperty]
    private string _guardianPhone = string.Empty;

    // Change Password Form
    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _currentPasswordError = string.Empty;

    [ObservableProperty]
    private string _newPasswordError = string.Empty;

    [ObservableProperty]
    private string _confirmPasswordError = string.Empty;

    [ObservableProperty]
    private string _passwordStatusMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity _passwordStatusSeverity = InfoBarSeverity.Informational;

    public StudentAccountViewModel(
        IStudentService studentService,
        IAdminService adminService,
        IUserSession userSession)
    {
        _studentService = studentService;
        _adminService = adminService;
        _userSession = userSession;
    }

    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        IsLoading = true;
        ClearPasswordForm();

        var user = _userSession.CurrentUser;
        if (user == null)
        {
            IsLoading = false;
            return;
        }

        Username = user.Username;
        StudentNo = user.StudentNo ?? string.Empty;
        FullName = $"{user.LastName}, {user.FirstName} {user.MiddleName}".Trim();
        Email = user.Email;
        Phone = user.Phone;
        AccountStatus = user.Status.ToString();

        // Load ID Submission details if available
        var submission = await _studentService.GetSubmissionAsync(user.Id);
        if (submission != null)
        {
            Gender = submission.Gender;
            BirthDate = submission.BirthDateDisplay;
            int calculatedAge = submission.ComputeAge();
            Age = calculatedAge.ToString();

            var sections = await _adminService.GetAllSectionsAsync();
            var sec = sections.FirstOrDefault(s => s.Id == submission.SectionId);
            YearAndSection = sec?.DisplayName ?? $"Section #{submission.SectionId}";
            Adviser = submission.AdviserSnapshot;
            Address = submission.Address;
            GuardianName = submission.GuardianName;
            GuardianPhone = submission.GuardianPhone;
        }
        else
        {
            Gender = string.Empty;
            BirthDate = string.Empty;
            Age = string.Empty;
            YearAndSection = string.Empty;
            Adviser = string.Empty;
            Address = string.Empty;
            GuardianName = string.Empty;
            GuardianPhone = string.Empty;
        }

        IsLoading = false;
    }

    [RelayCommand]
    public async Task ChangePasswordAsync()
    {
        ClearErrors();
        PasswordStatusMessage = string.Empty;

        var user = _userSession.CurrentUser;
        if (user == null) return;

        var result = await _studentService.ChangePasswordAsync(user.Id, CurrentPassword, NewPassword, ConfirmPassword);
        if (!result.IsValid)
        {
            if (result.Errors.TryGetValue("currentPassword", out var cpErr)) CurrentPasswordError = cpErr;
            if (result.Errors.TryGetValue("newPassword", out var npErr)) NewPasswordError = npErr;
            if (result.Errors.TryGetValue("confirmPassword", out var confErr)) ConfirmPasswordError = confErr;
            return;
        }

        ClearPasswordForm();
        PasswordStatusMessage = "Your password has been updated successfully.";
        PasswordStatusSeverity = InfoBarSeverity.Success;
    }

    private void ClearPasswordForm()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        ClearErrors();
    }

    private void ClearErrors()
    {
        CurrentPasswordError = string.Empty;
        NewPasswordError = string.Empty;
        ConfirmPasswordError = string.Empty;
    }
}
