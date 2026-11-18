using IdGen.App.ViewModels.Admin;
using IdGen.App.Views.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views;

public sealed partial class AdminShellPage : Page
{
    public AdminShellViewModel ViewModel { get; }

    public AdminShellPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<AdminShellViewModel>();
        InitializeComponent();
    }

    private void AdminNavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Select first item by default
        AdminNavView.SelectedItem = NavItemRegistrations;
        NavigateTo("Registrations");
    }

    private void AdminNavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is NavigationViewItem item && item.Tag is string tag)
        {
            if (tag == "SignOut")
            {
                ViewModel.SignOut();
                return;
            }

            NavigateTo(tag);
        }
    }

    private void NavigateTo(string tag)
    {
        Type pageType = tag switch
        {
            "Registrations" => typeof(AdminRegistrationsPage),
            "Sections" => typeof(AdminSectionsPage),
            "Defaults" => typeof(AdminDefaultsPage),
            "Designer" => typeof(AdminTemplateDesignerPage),
            "Submissions" => typeof(AdminSubmissionsPage),
            "Resets" => typeof(AdminResetRequestsPage),
            "Users" => typeof(AdminUsersPage),
            _ => typeof(AdminRegistrationsPage)
        };

        if (AdminContentFrame.CurrentSourcePageType != pageType)
        {
            AdminContentFrame.Navigate(pageType);
            AdminNavView.Header = tag switch
            {
                "Registrations" => "Student Registrations",
                "Sections" => "Year & Section Management",
                "Defaults" => "System Defaults",
                "Designer" => "ID Template Designer",
                "Submissions" => "Student ID Submissions",
                "Resets" => "Reset ID Requests",
                "Users" => "System Users",
                _ => "Admin Portal"
            };
        }
    }
}
