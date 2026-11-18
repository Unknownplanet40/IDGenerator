using IdGen.App.ViewModels.Student;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views.Student;

public sealed partial class StudentResetIdPage : Page
{
    public StudentResetIdViewModel ViewModel { get; }

    public StudentResetIdPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<StudentResetIdViewModel>();
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadStateAsync();
    }
}
