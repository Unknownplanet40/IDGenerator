using System.Runtime.InteropServices.WindowsRuntime;
using IdGen.App.Controls;
using IdGen.App.Services;
using IdGen.App.ViewModels.Admin;
using IdGen.App.Views.Common;
using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Models.Template;
using IdGen.Core.Services;
using IdGen.Core.Validation;
using IdGen.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.UI;

namespace IdGen.App.Views.Admin;

public sealed partial class AdminSubmissionsPage : Page
{
    public AdminSubmissionsViewModel ViewModel { get; }

    public AdminSubmissionsPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<AdminSubmissionsViewModel>();
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            try
            {
                await ViewModel.LoadSubmissionsCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                App.LogAppMessage($"AdminSubmissionsPage.Loaded exception: {ex}");
            }
        };
    }

    private async void OnApproveClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: IdSubmission submission })
        {
            await ViewModel.ApproveCommand.ExecuteAsync(submission);
        }
    }

    private async void OnDisapproveClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: IdSubmission submission })
        {
            await ViewModel.DisapproveCommand.ExecuteAsync(submission);
        }
    }

    private async void OnPreviewClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.DataContext is not IdSubmission submission) return;

        try
        {
            var templateService = App.CurrentServices.GetRequiredService<ITemplateService>();
            var assetRepo = App.CurrentServices.GetRequiredService<ITemplateAssetRepository>();

            var record = await templateService.GetTemplateAsync();
            var frontSide = TemplateValidator.ParseOrDefault(record.FrontJson, isFront: true);
            var backSide = TemplateValidator.ParseOrDefault(record.BackJson, isFront: false);

            double canvasW = record.Orientation == "Landscape"
                ? TemplateConstants.StandardLandscapeWidth
                : TemplateConstants.StandardPortraitWidth;
            double canvasH = record.Orientation == "Landscape"
                ? TemplateConstants.StandardLandscapeHeight
                : TemplateConstants.StandardPortraitHeight;

            // Load Assets
            BitmapImage? logoImg = null;
            var logoAsset = await assetRepo.GetActiveAssetByKindAsync("SchoolLogo");
            if (logoAsset != null && logoAsset.Data.Length > 0)
            {
                logoImg = await CreateBitmapFromBytesAsync(logoAsset.Data);
            }

            BitmapImage? frontBgImg = null;
            if (frontSide.Background.AssetId.HasValue)
            {
                var asset = await assetRepo.GetByIdAsync(frontSide.Background.AssetId.Value);
                if (asset != null && asset.Data.Length > 0)
                {
                    frontBgImg = await CreateBitmapFromBytesAsync(asset.Data);
                }
            }

            BitmapImage? backBgImg = null;
            if (backSide.Background.AssetId.HasValue)
            {
                var asset = await assetRepo.GetByIdAsync(backSide.Background.AssetId.Value);
                if (asset != null && asset.Data.Length > 0)
                {
                    backBgImg = await CreateBitmapFromBytesAsync(asset.Data);
                }
            }

            BitmapImage? photoImg = (submission.PhotoPng != null && submission.PhotoPng.Length > 0)
                ? await CreateBitmapFromBytesAsync(submission.PhotoPng)
                : null;

            BitmapImage? sigImg = (submission.SignaturePng != null && submission.SignaturePng.Length > 0)
                ? await CreateBitmapFromBytesAsync(submission.SignaturePng)
                : null;

            // Resolve Field Values from snapshots
            var fields = BuildFieldValues(submission);

            bool isPortrait = record.Orientation != "Landscape";
            var (frontVisual, frontCanvas) = BuildCardVisual(frontSide, canvasW, canvasH, fields, logoImg, photoImg, sigImg, frontBgImg);
            var (backVisual, backCanvas) = BuildCardVisual(backSide, canvasW, canvasH, fields, logoImg, photoImg, sigImg, backBgImg);

            // Dialog content
            var contentStack = new StackPanel { Spacing = 16 };

            // Student summary banner with Print / Export action
            var banner = new Border
            {
                Background = (Brush)Application.Current.Resources["CardBackgroundFillColorSecondaryBrush"],
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 8, 12, 8)
            };
            var bannerGrid = new Grid();
            bannerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            bannerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var bannerText = new TextBlock
            {
                Text = $"{submission.StudentNameDisplay}  •  {submission.StudentNoDisplay}  •  {submission.CourseSnapshot ?? "General"}  •  Status: {submission.Status}",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(bannerText, 0);
            bannerGrid.Children.Add(bannerText);

            var previewPrintBtn = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Children =
                    {
                        new FontIcon { Glyph = "\uE749", FontSize = 13 },
                        new TextBlock { Text = "Print / Export ID", FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold }
                    }
                },
                VerticalAlignment = VerticalAlignment.Center
            };
            previewPrintBtn.Click += async (s, e) =>
            {
                var printService = App.CurrentServices.GetRequiredService<CardPrintService>();
                IntPtr hwnd = App.MainWindowInstance != null ? WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance) : IntPtr.Zero;
                var res = await printService.PrintOrExportCardAsync(
                    this.XamlRoot,
                    hwnd,
                    submission.StudentNameDisplay,
                    submission.StudentNoDisplay,
                    frontCanvas,
                    backCanvas,
                    (int)canvasW,
                    (int)canvasH,
                    isPortrait);

                if (!res.IsCancelled)
                {
                    var msgDialog = new ContentDialog
                    {
                        Title = res.Success ? "Print / Export Complete" : "Print / Export Failed",
                        Content = res.Message,
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await msgDialog.ShowAsync();
                }
            };
            Grid.SetColumn(previewPrintBtn, 1);
            bannerGrid.Children.Add(previewPrintBtn);

            banner.Child = bannerGrid;
            contentStack.Children.Add(banner);

            var cardsRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 24,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var frontCol = new StackPanel { Spacing = 8, HorizontalAlignment = HorizontalAlignment.Center };
            frontCol.Children.Add(new TextBlock { Text = "FRONT SIDE", FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
            frontCol.Children.Add(frontVisual);

            var backCol = new StackPanel { Spacing = 8, HorizontalAlignment = HorizontalAlignment.Center };
            backCol.Children.Add(new TextBlock { Text = "BACK SIDE", FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
            backCol.Children.Add(backVisual);

            cardsRow.Children.Add(frontCol);
            cardsRow.Children.Add(backCol);

            var scroll = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = cardsRow,
                Padding = new Thickness(8, 0, 8, 12)
            };
            contentStack.Children.Add(scroll);

            var dialog = new ContentDialog
            {
                Title = $"ID Card Review - Form #{submission.Id}",
                Content = contentStack,
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            if (submission.IsPending)
            {
                dialog.PrimaryButtonText = "Approve";
                dialog.SecondaryButtonText = "Disapprove";
                dialog.DefaultButton = ContentDialogButton.Primary;
            }

            var dialogResult = await dialog.ShowAsync();
            if (dialogResult == ContentDialogResult.Primary)
            {
                await ViewModel.ApproveCommand.ExecuteAsync(submission);
            }
            else if (dialogResult == ContentDialogResult.Secondary)
            {
                await ViewModel.DisapproveCommand.ExecuteAsync(submission);
            }
        }
        catch (Exception ex)
        {
            var errDialog = new ContentDialog
            {
                Title = "Preview Error",
                Content = $"Failed to preview ID card: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errDialog.ShowAsync();
        }
    }

    private async void OnPrintRowClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is IdSubmission submission)
        {
            await PrintSingleSubmissionAsync(submission);
        }
    }

    private async Task PrintSingleSubmissionAsync(IdSubmission submission)
    {
        try
        {
            var templateService = App.CurrentServices.GetRequiredService<ITemplateService>();
            var assetRepo = App.CurrentServices.GetRequiredService<ITemplateAssetRepository>();
            var printService = App.CurrentServices.GetRequiredService<CardPrintService>();

            var record = await templateService.GetTemplateAsync();
            var frontSide = TemplateValidator.ParseOrDefault(record.FrontJson, isFront: true);
            var backSide = TemplateValidator.ParseOrDefault(record.BackJson, isFront: false);

            bool isPortrait = record.Orientation != "Landscape";
            double canvasW = isPortrait ? TemplateConstants.StandardPortraitWidth : TemplateConstants.StandardLandscapeWidth;
            double canvasH = isPortrait ? TemplateConstants.StandardPortraitHeight : TemplateConstants.StandardLandscapeHeight;

            BitmapImage? logoImg = null;
            var logoAsset = await assetRepo.GetActiveAssetByKindAsync("SchoolLogo");
            if (logoAsset != null && logoAsset.Data.Length > 0)
                logoImg = await CreateBitmapFromBytesAsync(logoAsset.Data);

            BitmapImage? frontBgImg = null;
            if (frontSide.Background.AssetId.HasValue)
            {
                var asset = await assetRepo.GetByIdAsync(frontSide.Background.AssetId.Value);
                if (asset != null && asset.Data.Length > 0)
                    frontBgImg = await CreateBitmapFromBytesAsync(asset.Data);
            }

            BitmapImage? backBgImg = null;
            if (backSide.Background.AssetId.HasValue)
            {
                var asset = await assetRepo.GetByIdAsync(backSide.Background.AssetId.Value);
                if (asset != null && asset.Data.Length > 0)
                    backBgImg = await CreateBitmapFromBytesAsync(asset.Data);
            }

            BitmapImage? photoImg = (submission.PhotoPng != null && submission.PhotoPng.Length > 0)
                ? await CreateBitmapFromBytesAsync(submission.PhotoPng) : null;
            BitmapImage? sigImg = (submission.SignaturePng != null && submission.SignaturePng.Length > 0)
                ? await CreateBitmapFromBytesAsync(submission.SignaturePng) : null;

            var fields = BuildFieldValues(submission);
            var (_, frontCanvas) = BuildCardVisual(frontSide, canvasW, canvasH, fields, logoImg, photoImg, sigImg, frontBgImg);
            var (_, backCanvas) = BuildCardVisual(backSide, canvasW, canvasH, fields, logoImg, photoImg, sigImg, backBgImg);

            IntPtr hwnd = App.MainWindowInstance != null ? WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance) : IntPtr.Zero;

            var res = await printService.PrintOrExportCardAsync(
                this.XamlRoot,
                hwnd,
                submission.StudentNameDisplay,
                submission.StudentNoDisplay,
                frontCanvas,
                backCanvas,
                (int)canvasW,
                (int)canvasH,
                isPortrait);

            if (!res.IsCancelled)
            {
                var resDialog = new ContentDialog
                {
                    Title = res.Success ? "Print / Export Complete" : "Print / Export Failed",
                    Content = res.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await resDialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var errDialog = new ContentDialog
            {
                Title = "Print Error",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errDialog.ShowAsync();
        }
    }

    private async void OnBatchPrintClicked(object sender, RoutedEventArgs e)
    {
        var approved = ViewModel.Submissions.Where(s => s.Status == SubmissionStatus.Approved).ToList();
        if (approved.Count == 0)
        {
            var noApprovedDialog = new ContentDialog
            {
                Title = "No Approved Submissions",
                Content = "There are currently no approved student ID submissions to print. Approve pending submissions first.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await noApprovedDialog.ShowAsync();
            return;
        }

        var batchDialog = new PrintCardOptionDialog
        {
            Title = $"Batch Print / Export ({approved.Count} Approved Students)",
            XamlRoot = this.XamlRoot
        };

        var choice = await batchDialog.ShowAsync();
        if (choice == ContentDialogResult.None) return;

        var side = batchDialog.SelectedSide;
        IntPtr hwnd = App.MainWindowInstance != null ? WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance) : IntPtr.Zero;

        try
        {
            var templateService = App.CurrentServices.GetRequiredService<ITemplateService>();
            var assetRepo = App.CurrentServices.GetRequiredService<ITemplateAssetRepository>();

            var template = await templateService.GetTemplateAsync();
            var frontSide = TemplateValidator.ParseOrDefault(template.FrontJson, isFront: true);
            var backSide = TemplateValidator.ParseOrDefault(template.BackJson, isFront: false);

            bool isPortrait = template.Orientation != "Landscape";
            double canvasW = isPortrait ? TemplateConstants.StandardPortraitWidth : TemplateConstants.StandardLandscapeWidth;
            double canvasH = isPortrait ? TemplateConstants.StandardPortraitHeight : TemplateConstants.StandardLandscapeHeight;

            BitmapImage? logoImg = null;
            var logoAsset = await assetRepo.GetActiveAssetByKindAsync("SchoolLogo");
            if (logoAsset != null && logoAsset.Data.Length > 0)
                logoImg = await CreateBitmapFromBytesAsync(logoAsset.Data);

            BitmapImage? frontBgImg = null;
            if (frontSide.Background.AssetId.HasValue)
            {
                var bgAsset = await assetRepo.GetByIdAsync(frontSide.Background.AssetId.Value);
                if (bgAsset != null && bgAsset.Data.Length > 0)
                    frontBgImg = await CreateBitmapFromBytesAsync(bgAsset.Data);
            }

            BitmapImage? backBgImg = null;
            if (backSide.Background.AssetId.HasValue)
            {
                var bgAsset = await assetRepo.GetByIdAsync(backSide.Background.AssetId.Value);
                if (bgAsset != null && bgAsset.Data.Length > 0)
                    backBgImg = await CreateBitmapFromBytesAsync(bgAsset.Data);
            }

            var printPages = new List<CardPrintPage>();
            var exportFiles = new List<(string FileName, byte[] PngBytes)>();

            foreach (var sub in approved)
            {
                BitmapImage? photoImg = (sub.PhotoPng != null && sub.PhotoPng.Length > 0)
                    ? await CreateBitmapFromBytesAsync(sub.PhotoPng) : null;
                BitmapImage? sigImg = (sub.SignaturePng != null && sub.SignaturePng.Length > 0)
                    ? await CreateBitmapFromBytesAsync(sub.SignaturePng) : null;

                var fields = BuildFieldValues(sub);

                if (side == CardPrintSideOption.BothSides || side == CardPrintSideOption.FrontOnly)
                {
                    var (_, fCanvas) = BuildCardVisual(frontSide, canvasW, canvasH, fields, logoImg, photoImg, sigImg, frontBgImg);
                    var rendered = await CardExportHelper.RenderElementToBgraAsync(fCanvas, (int)canvasW, (int)canvasH);
                    if (rendered.HasValue)
                    {
                        printPages.Add(new CardPrintPage(rendered.Value.PixelBytes, rendered.Value.Width, rendered.Value.Height, isPortrait));
                        if (choice == ContentDialogResult.Secondary)
                        {
                            var png = await CardExportHelper.EncodeBgraToPngBytesAsync(rendered.Value.PixelBytes, rendered.Value.Width, rendered.Value.Height);
                            if (png != null)
                                exportFiles.Add(($"{sub.StudentNoDisplay}_Front_CR80.png", png));
                        }
                    }
                }

                if (side == CardPrintSideOption.BothSides || side == CardPrintSideOption.BackOnly)
                {
                    var (_, bCanvas) = BuildCardVisual(backSide, canvasW, canvasH, fields, logoImg, photoImg, sigImg, backBgImg);
                    var rendered = await CardExportHelper.RenderElementToBgraAsync(bCanvas, (int)canvasW, (int)canvasH);
                    if (rendered.HasValue)
                    {
                        printPages.Add(new CardPrintPage(rendered.Value.PixelBytes, rendered.Value.Width, rendered.Value.Height, isPortrait));
                        if (choice == ContentDialogResult.Secondary)
                        {
                            var png = await CardExportHelper.EncodeBgraToPngBytesAsync(rendered.Value.PixelBytes, rendered.Value.Width, rendered.Value.Height);
                            if (png != null)
                                exportFiles.Add(($"{sub.StudentNoDisplay}_Back_CR80.png", png));
                        }
                    }
                }
            }

            if (choice == ContentDialogResult.Primary)
            {
                bool success = NativePrintHelper.PrintPages(hwnd, $"Batch Student IDs ({approved.Count} students)", printPages);
                var resDialog = new ContentDialog
                {
                    Title = success ? "Batch Print Complete" : "Print Cancelled",
                    Content = success ? $"Sent {printPages.Count} card pages to printer." : "Batch print cancelled.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await resDialog.ShowAsync();
            }
            else if (choice == ContentDialogResult.Secondary)
            {
                string? folder = await CardExportHelper.SaveMultiplePngsToFolderPickerAsync(exportFiles, hwnd);
                var resDialog = new ContentDialog
                {
                    Title = folder != null ? "Batch Export Complete" : "Export Cancelled",
                    Content = folder != null ? $"Exported {exportFiles.Count} card images to:\n{folder}" : "Folder selection cancelled.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await resDialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var errDialog = new ContentDialog
            {
                Title = "Batch Error",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errDialog.ShowAsync();
        }
    }

    private static Dictionary<string, string> BuildFieldValues(IdSubmission submission)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["student_no"] = submission.StudentNoDisplay,
            ["full_name"] = submission.StudentNameDisplay,
            ["gender"] = submission.Gender,
            ["birth_date"] = submission.BirthDate.ToString("MMMM dd, yyyy"),
            ["blood_type"] = string.IsNullOrWhiteSpace(submission.BloodType) ? "N/A" : submission.BloodType,
            ["address"] = submission.Address,
            ["guardian_name"] = submission.GuardianName,
            ["guardian_phone"] = submission.GuardianPhone,
            ["school_name"] = submission.SchoolSnapshot,
            ["campus_name"] = submission.CampusSnapshot ?? "",
            ["principal_name"] = submission.PrincipalSnapshot,
            ["principal_title"] = submission.PrincipalTitleSnapshot ?? "",
            ["semester"] = TemplateConstants.FormatSemester(submission.SemesterSnapshot),
            ["school_year"] = submission.SchoolYearSnapshot ?? "",
            ["course"] = submission.CourseSnapshot ?? "",
            ["adviser"] = submission.AdviserSnapshot
        };
    }

    private static (FrameworkElement Visual, Canvas Canvas) BuildCardVisual(
        TemplateSideDefinition side,
        double canvasWidth,
        double canvasHeight,
        Dictionary<string, string> fieldValues,
        ImageSource? logoSource,
        ImageSource? photoSource,
        ImageSource? signatureSource,
        ImageSource? backgroundSource)
    {
        var canvas = new Canvas
        {
            Width = canvasWidth,
            Height = canvasHeight
        };

        // Background
        var bgBorder = new Border
        {
            Width = canvasWidth,
            Height = canvasHeight,
            Background = ParseHexBrush(side.Background.Color)
        };
        if (backgroundSource != null)
        {
            bgBorder.Child = new Image
            {
                Source = backgroundSource,
                Stretch = side.Background.Fit switch
                {
                    "Stretch" => Stretch.Fill,
                    "Contain" => Stretch.Uniform,
                    _ => Stretch.UniformToFill
                }
            };
        }
        canvas.Children.Add(bgBorder);

        // Elements
        foreach (var el in side.Elements)
        {
            string sampleText = el.Type == "DynamicText" && !string.IsNullOrWhiteSpace(el.FieldKey) && fieldValues.TryGetValue(el.FieldKey, out var val)
                ? val
                : (el.StaticText ?? "");

            var view = new TemplateElementView
            {
                Element = el,
                SampleText = sampleText,
                LogoSource = logoSource,
                PhotoSource = photoSource,
                SignatureSource = signatureSource,
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

            canvas.Children.Add(wrapper);
        }

        double displayW = canvasWidth * 0.38;
        double displayH = canvasHeight * 0.38;

        var viewbox = new Viewbox
        {
            Width = displayW,
            Height = displayH,
            Stretch = Stretch.Uniform,
            Child = canvas
        };

        var cardBorder = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(180, 148, 163, 184)),
            Background = new SolidColorBrush(Colors.White),
            Child = viewbox
        };

        return (cardBorder, canvas);
    }

    private static async Task<BitmapImage?> CreateBitmapFromBytesAsync(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
        try
        {
            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(bytes.AsBuffer());
            stream.Seek(0);
            var bmp = new BitmapImage();
            await bmp.SetSourceAsync(stream);
            return bmp;
        }
        catch
        {
            return null;
        }
    }

    private static SolidColorBrush ParseHexBrush(string? hex)
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
}
