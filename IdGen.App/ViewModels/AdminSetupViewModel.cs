using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.App.Services;
using IdGen.Core.Services;
using IdGen.Core.Validation;

namespace IdGen.App.ViewModels;

public partial class AdminSetupViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IAppNavigationService _navigationService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string? _usernameError;

    [ObservableProperty]
    private string? _passwordError;

    [ObservableProperty]
    private string? _confirmPasswordError;

    [ObservableProperty]
    private bool _isLoading;

    public AdminSetupViewModel(IAuthService authService, IAppNavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task CreateAdminAsync()
    {
        ClearErrors();

        var validation = InputValidators.ValidateAdminSetup(Username, Password, ConfirmPassword);
        if (!validation.IsValid)
        {
            UsernameError = validation.GetError(nameof(Username));
            PasswordError = validation.GetError(nameof(Password));
            ConfirmPasswordError = validation.GetError(nameof(ConfirmPassword));
            ErrorMessage = "Please correct the errors before proceeding.";
            HasError = true;
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _authService.CreateFirstAdminAsync(Username, Password, ConfirmPassword);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to create administrator account.";
                HasError = true;
                return;
            }

            // Navigate to login page
            _navigationService.NavigateTo(typeof(Views.LoginPage));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearErrors()
    {
        UsernameError = null;
        PasswordError = null;
        ConfirmPasswordError = null;
        ErrorMessage = string.Empty;
        HasError = false;
    }
}
