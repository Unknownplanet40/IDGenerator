using IdGen.App.ViewModels.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views.Admin;

public sealed partial class AdminDefaultsPage : Page
{
    public AdminDefaultsViewModel ViewModel { get; }

    public AdminDefaultsPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<AdminDefaultsViewModel>();
        InitializeComponent();
        Loaded += async (s, e) => await ViewModel.LoadDefaultsCommand.ExecuteAsync(null);
    }
}
