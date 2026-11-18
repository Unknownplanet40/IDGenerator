using IdGen.App.ViewModels.Admin;
using IdGen.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views.Admin;

public sealed partial class AdminResetRequestsPage : Page
{
    public AdminResetRequestsViewModel ViewModel { get; }

    public AdminResetRequestsPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<AdminResetRequestsViewModel>();
        InitializeComponent();
        Loaded += async (s, e) => await ViewModel.LoadResetRequestsCommand.ExecuteAsync(null);
    }

    private async void OnFilterPendingClicked(object sender, RoutedEventArgs e)
    {
        BtnFilterPending.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
        BtnFilterAll.ClearValue(Button.StyleProperty);
        await ViewModel.SetFilterAsync("Pending");
    }

    private async void OnFilterAllClicked(object sender, RoutedEventArgs e)
    {
        BtnFilterAll.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
        BtnFilterPending.ClearValue(Button.StyleProperty);
        await ViewModel.SetFilterAsync("All");
    }

    private async void OnApproveClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ResetRequest req })
        {
            await ViewModel.ApproveCommand.ExecuteAsync(req);
        }
    }

    private async void OnDeclineClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ResetRequest req })
        {
            await ViewModel.DeclineCommand.ExecuteAsync(req);
        }
    }
}
