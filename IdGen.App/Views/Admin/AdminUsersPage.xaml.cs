using IdGen.App.ViewModels.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views.Admin;

public sealed partial class AdminUsersPage : Page
{
    public AdminUsersViewModel ViewModel { get; }

    public AdminUsersPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<AdminUsersViewModel>();
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            try
            {
                if (ViewModel != null)
                {
                    await ViewModel.LoadUsersCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                App.LogAppMessage($"AdminUsersPage.Loaded error: {ex}");
            }
        };
    }

    private async void OnRefreshClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            await ViewModel.LoadUsersCommand.ExecuteAsync(null);
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (ViewModel != null && sender is TextBox tb)
        {
            ViewModel.SearchCommand.Execute(tb.Text);
        }
    }

    private void OnRoleFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null && sender is ComboBox cb && cb.SelectedItem is ComboBoxItem item && item.Tag is string role)
        {
            ViewModel.SetRoleFilterCommand.Execute(role);
        }
    }

    private void OnStatusFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null && sender is ComboBox cb && cb.SelectedItem is ComboBoxItem item && item.Tag is string status)
        {
            ViewModel.SetStatusFilterCommand.Execute(status);
        }
    }
}
