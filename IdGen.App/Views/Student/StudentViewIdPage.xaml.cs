using IdGen.App.Controls;
using IdGen.App.Services;
using IdGen.App.ViewModels.Student;
using IdGen.Core.Models.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace IdGen.App.Views.Student;

public sealed partial class StudentViewIdPage : Page
{
    public StudentViewIdViewModel ViewModel { get; }

    public StudentViewIdPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<StudentViewIdViewModel>();
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadSubmissionCommand.ExecuteAsync(null);
                RenderCards();
            }
        }
        catch (Exception ex)
        {
            App.LogAppMessage($"StudentViewIdPage.Page_Loaded exception: {ex}");
        }
    }

    private void RenderCards()
    {
        if (FrontElementsCanvas == null || BackElementsCanvas == null) return;

        FrontElementsCanvas.Children.Clear();
        BackElementsCanvas.Children.Clear();

        if (ViewModel.FrontBackgroundColor != null)
        {
            FrontBackgroundBorder.Background = ParseHexBrush(ViewModel.FrontBackgroundColor);
        }

        if (ViewModel.BackBackgroundColor != null)
        {
            BackBackgroundBorder.Background = ParseHexBrush(ViewModel.BackBackgroundColor);
        }

        // Render Front Elements
        foreach (var el in ViewModel.FrontSide.Elements)
        {
            var wrapper = CreateElementDisplay(el);
            FrontElementsCanvas.Children.Add(wrapper);
        }

        // Render Back Elements
        foreach (var el in ViewModel.BackSide.Elements)
        {
            var wrapper = CreateElementDisplay(el);
            BackElementsCanvas.Children.Add(wrapper);
        }
    }

    private FrameworkElement CreateElementDisplay(TemplateElementDefinition el)
    {
        var view = new TemplateElementView
        {
            Element = el,
            SampleText = ViewModel.ResolveText(el),
            LogoSource = ViewModel.SchoolLogoImage,
            SignatureSource = ViewModel.StudentSignatureImage,
            PhotoSource = ViewModel.StudentPhotoImage,
            IsSelected = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var wrapper = new Grid
        {
            Width = el.Width,
            Height = el.Height
        };

        Canvas.SetLeft(wrapper, el.X);
        Canvas.SetTop(wrapper, el.Y);

        wrapper.Children.Add(view);
        return wrapper;
    }

    public double GetCardDisplayWidth(double width) => width * 0.48;
    public double GetCardDisplayHeight(double height) => height * 0.48;

    public Stretch GetImageStretch(string fit) => fit switch
    {
        "Stretch" => Stretch.Fill,
        "Contain" => Stretch.Uniform,
        _ => Stretch.UniformToFill
    };

    private static SolidColorBrush ParseHexBrush(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return new SolidColorBrush(Colors.White);
        string clean = hex.Trim().TrimStart('#');
        byte r = 255, g = 255, b = 255;
        if (clean.Length == 6)
        {
            r = Convert.ToByte(clean.Substring(0, 2), 16);
            g = Convert.ToByte(clean.Substring(2, 2), 16);
            b = Convert.ToByte(clean.Substring(4, 2), 16);
        }
        return new SolidColorBrush(Color.FromArgb(255, r, g, b));
    }

    private async void PrintCardButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var printService = App.CurrentServices.GetRequiredService<CardPrintService>();
            IntPtr hwnd = App.MainWindowInstance != null ? WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance) : IntPtr.Zero;

            bool isPortrait = !string.Equals(ViewModel.Orientation, "Landscape", StringComparison.OrdinalIgnoreCase);
            int canvasW = (int)ViewModel.CanvasWidth;
            int canvasH = (int)ViewModel.CanvasHeight;

            var res = await printService.PrintOrExportCardAsync(
                this.XamlRoot,
                hwnd,
                ViewModel.FullName,
                ViewModel.StudentNo,
                FrontCanvas,
                BackCanvas,
                canvasW,
                canvasH,
                isPortrait);

            if (!res.IsCancelled)
            {
                var dialog = new ContentDialog
                {
                    Title = res.Success ? "Print / Export Complete" : "Print / Export Failed",
                    Content = res.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var errDialog = new ContentDialog
            {
                Title = "Error",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errDialog.ShowAsync();
        }
    }
}
