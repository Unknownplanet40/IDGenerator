using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.App.Services;
using IdGen.Core.Services;
using IdGen.Core.Validation;

namespace IdGen.App.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IAppNavigationService _navigationService;

    [ObservableProperty] private string _studentNo = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _middleName = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    [ObservableProperty] private string? _studentNoError;
    [ObservableProperty] private string? _lastNameError;
    [ObservableProperty] private string? _firstNameError;
    [ObservableProperty] private string? _middleNameError;
    [ObservableProperty] private string? _emailError;
    [ObservableProperty] private string? _phoneError;
    [ObservableProperty] private string? _usernameError;
    [ObservableProperty] private string? _passwordError;
    [ObservableProperty] private string? _confirmPasswordError;

    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _successMessage = string.Empty;
    [ObservableProperty] private bool _hasSuccess;
    [ObservableProperty] private bool _isLoading;

    public RegisterViewModel(IAuthService authService, IAppNavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task RegisterAsync()
    {
        ClearErrors();

        // A4: Empty fields inline validation
        var validation = InputValidators.ValidateRegistration(
            StudentNo, LastName, FirstName, MiddleName, Email, Phone, Username, Password, ConfirmPassword);

        if (!validation.IsValid)
        {
            AssignValidationErrors(validation);
            ErrorMessage = "Please resolve the form errors before submitting.";
            HasError = true;
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _authService.RegisterStudentAsync(
                StudentNo, LastName, FirstName, MiddleName, Email, Phone, Username, Password, ConfirmPassword);

            if (!result.IsValid)
            {
                AssignValidationErrors(result);
                ErrorMessage = result.Errors.Values.FirstOrDefault() ?? "Registration failed.";
                HasError = true;
                return;
            }

            HasSuccess = true;
            SuccessMessage = "Registration submitted successfully! Your account is pending administrator approval.";

            // Short pause or user can click Go To Login
            await Task.Delay(1500);
            _navigationService.NavigateTo(typeof(Views.LoginPage));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void GoToLogin()
    {
        _navigationService.NavigateTo(typeof(Views.LoginPage));
    }

    private void AssignValidationErrors(ValidationResult validation)
    {
        StudentNoError = validation.GetError(nameof(StudentNo));
        LastNameError = validation.GetError(nameof(LastName));
        FirstNameError = validation.GetError(nameof(FirstName));
        MiddleNameError = validation.GetError(nameof(MiddleName));
        EmailError = validation.GetError(nameof(Email));
        PhoneError = validation.GetError(nameof(Phone));
        UsernameError = validation.GetError(nameof(Username));
        PasswordError = validation.GetError(nameof(Password));
        ConfirmPasswordError = validation.GetError(nameof(ConfirmPassword));
    }

    private void ClearErrors()
    {
        StudentNoError = null;
        LastNameError = null;
        FirstNameError = null;
        MiddleNameError = null;
        EmailError = null;
        PhoneError = null;
        UsernameError = null;
        PasswordError = null;
        ConfirmPasswordError = null;
        ErrorMessage = string.Empty;
        HasError = false;
        SuccessMessage = string.Empty;
        HasSuccess = false;
    }
}
