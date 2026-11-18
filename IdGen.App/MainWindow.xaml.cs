using IdGen.App.Services;
using IdGen.App.Views;
using IdGen.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace IdGen.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        App.LogAppMessage("MW.1 Before MainWindow.InitializeComponent");
        InitializeComponent();
        App.LogAppMessage("MW.2 After MainWindow.InitializeComponent");

        try
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            App.LogAppMessage("MW.3 TitleBar configured");

            if (AppWindow != null)
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(1280, 800));
            }
        }
        catch (Exception ex)
        {
            App.LogAppMessage("MW.3 TitleBar exception: " + ex.ToString());
        }

        var navigationService = App.CurrentServices.GetRequiredService<IAppNavigationService>();
        navigationService.Initialize(RootFrame);
        App.LogAppMessage("MW.4 NavigationService initialized");

        InitialNavigation();
        App.LogAppMessage("MW.5 InitialNavigation initiated");
    }

    private async void InitialNavigation()
    {
        try
        {
            App.LogAppMessage("IN.1 InitialNavigation entered");
            var authService = App.CurrentServices.GetRequiredService<IAuthService>();
            var navigationService = App.CurrentServices.GetRequiredService<IAppNavigationService>();

            bool hasAdmin = await authService.HasAnyAdminAsync();
            App.LogAppMessage("IN.2 hasAdmin = " + hasAdmin);
            if (hasAdmin)
            {
                navigationService.NavigateTo(typeof(LoginPage));
            }
            else
            {
                navigationService.NavigateTo(typeof(AdminSetupPage));
            }
            App.LogAppMessage("IN.3 Navigation complete");
        }
        catch (Exception ex)
        {
            App.LogAppMessage("IN.Exception: " + ex.ToString());
        }
    }
}
