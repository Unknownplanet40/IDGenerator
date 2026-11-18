using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.App.Services;

namespace IdGen.App.ViewModels.Student;

public partial class StudentShellViewModel : ObservableObject
{
    private readonly IAppNavigationService _navigationService;
    private readonly IUserSession _userSession;

    public StudentShellViewModel(IAppNavigationService navigationService, IUserSession userSession)
    {
        _navigationService = navigationService;
        _userSession = userSession;
    }

    [RelayCommand]
    public void SignOut()
    {
        _userSession.Clear();
        _navigationService.NavigateTo(typeof(Views.LoginPage));
    }
}
