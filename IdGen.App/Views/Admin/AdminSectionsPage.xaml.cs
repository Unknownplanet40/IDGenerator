using IdGen.App.ViewModels.Admin;
using IdGen.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views.Admin;

public sealed partial class AdminSectionsPage : Page
{
    public AdminSectionsViewModel ViewModel { get; }

    public AdminSectionsPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<AdminSectionsViewModel>();
        InitializeComponent();
        Loaded += async (s, e) => await ViewModel.LoadSectionsCommand.ExecuteAsync(null);
    }

    private void OnEditClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && sender is Button { DataContext: Section section })
        {
            ViewModel.BeginEditCommand.Execute(section);
        }
    }

    private async void OnDeleteClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && sender is Button { DataContext: Section section })
        {
            await ViewModel.DeleteSectionCommand.ExecuteAsync(section);
        }
    }
}
