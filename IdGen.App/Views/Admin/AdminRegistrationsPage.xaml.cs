using IdGen.App.ViewModels.Admin;
using IdGen.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views.Admin;

public sealed partial class AdminRegistrationsPage : Page
{
    public AdminRegistrationsViewModel ViewModel { get; }

    public AdminRegistrationsPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<AdminRegistrationsViewModel>();
        InitializeComponent();
        Loaded += async (s, e) => await ViewModel.LoadRegistrationsCommand.ExecuteAsync(null);
    }

    private async void OnApproveClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: User user })
        {
            await ViewModel.ApproveCommand.ExecuteAsync(user);
        }
    }

    private async void OnDeclineClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: User user })
        {
            await ViewModel.DeclineCommand.ExecuteAsync(user);
        }
    }
}
