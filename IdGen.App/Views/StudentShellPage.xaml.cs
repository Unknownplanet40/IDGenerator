using IdGen.App.ViewModels.Student;
using IdGen.App.Views.Student;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views;

public sealed partial class StudentShellPage : Page
{
    public StudentShellViewModel ViewModel { get; }

    public StudentShellPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<StudentShellViewModel>();
        InitializeComponent();
    }

    private void StudentNavView_Loaded(object sender, RoutedEventArgs e)
    {
        StudentNavView.SelectedItem = NavItemGenerator;
        NavigateTo("Generator");
    }

    private void StudentNavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
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
            "Generator" => typeof(StudentGeneratorPage),
            "ViewId" => typeof(StudentViewIdPage),
            "ResetId" => typeof(StudentResetIdPage),
            "Account" => typeof(StudentAccountPage),
            _ => typeof(StudentGeneratorPage)
        };

        if (StudentContentFrame.CurrentSourcePageType != pageType)
        {
            StudentContentFrame.Navigate(pageType);
            StudentNavView.Header = tag switch
            {
                "Generator" => "ID Generator",
                "ViewId" => "View Student ID",
                "ResetId" => "Reset ID Request",
                "Account" => "Account Settings",
                _ => "Student Portal"
            };
        }
    }
}
