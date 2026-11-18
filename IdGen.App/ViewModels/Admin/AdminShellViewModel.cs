using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.App.Services;

namespace IdGen.App.ViewModels.Admin;

public partial class AdminShellViewModel : ObservableObject
{
    private readonly IAppNavigationService _navigationService;
    private readonly IUserSession _userSession;

    public AdminShellViewModel(IAppNavigationService navigationService, IUserSession userSession)
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
