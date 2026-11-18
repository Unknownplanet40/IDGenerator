using IdGen.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views;

public sealed partial class AdminSetupPage : Page
{
    public AdminSetupViewModel ViewModel { get; }

    public AdminSetupPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<AdminSetupViewModel>();
        InitializeComponent();
    }
}
