using IdGen.App.ViewModels.Student;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views.Student;

public sealed partial class StudentAccountPage : Page
{
    public StudentAccountViewModel ViewModel { get; }

    public StudentAccountPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<StudentAccountViewModel>();
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadProfileAsync();
    }

    private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null) ViewModel.CurrentPassword = CurrentPasswordBox.Password;
    }

    private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null) ViewModel.NewPassword = NewPasswordBox.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null) ViewModel.ConfirmPassword = ConfirmPasswordBox.Password;
    }
}
