using IdGen.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views;

public sealed partial class RegisterPage : Page
{
    public RegisterViewModel ViewModel { get; }

    public RegisterPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<RegisterViewModel>();
        InitializeComponent();
    }
}
