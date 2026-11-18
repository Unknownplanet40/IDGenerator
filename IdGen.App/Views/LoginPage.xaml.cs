using IdGen.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<LoginViewModel>();
        InitializeComponent();
    }
}
