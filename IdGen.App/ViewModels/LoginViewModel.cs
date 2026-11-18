using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.App.Services;
using IdGen.Core.Enums;
using IdGen.Core.Services;

namespace IdGen.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IAppNavigationService _navigationService;
    private readonly IUserSession _userSession;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string? _usernameError;

    [ObservableProperty]
    private string? _passwordError;

    [ObservableProperty]
    private bool _isLoading;

    public LoginViewModel(
        IAuthService authService,
        IAppNavigationService navigationService,
        IUserSession userSession)
    {
        _authService = authService;
        _navigationService = navigationService;
        _userSession = userSession;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        ClearErrors();

        // A4: Empty fields inline validation, nothing submitted
        bool hasInlineError = false;
        if (string.IsNullOrWhiteSpace(Username))
        {
            UsernameError = "Username is required.";
            hasInlineError = true;
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = "Password is required.";
            hasInlineError = true;
        }

        if (hasInlineError)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _authService.LoginAsync(Username, Password);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage ?? "Login failed.";
                HasError = true;
                return;
            }

            _userSession.SetUser(result.User!);

            if (result.User!.Role == UserRole.Admin)
            {
                _navigationService.NavigateTo(typeof(Views.AdminShellPage));
            }
            else
            {
                _navigationService.NavigateTo(typeof(Views.StudentShellPage));
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void GoToRegister()
    {
        _navigationService.NavigateTo(typeof(Views.RegisterPage));
    }

    private void ClearErrors()
    {
        UsernameError = null;
        PasswordError = null;
        ErrorMessage = string.Empty;
        HasError = false;
    }
}
