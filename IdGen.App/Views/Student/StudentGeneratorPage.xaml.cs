using System.Runtime.InteropServices.WindowsRuntime;
using IdGen.App.ViewModels.Student;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace IdGen.App.Views.Student;

public sealed partial class StudentGeneratorPage : Page
{
    public StudentGeneratorViewModel ViewModel { get; }

    private bool _isDrawing;
    private Point _lastPoint;
    private bool _hasStrokes;

    public StudentGeneratorPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<StudentGeneratorViewModel>();
        InitializeComponent();
        ViewModel.SignatureProvider = GetSignaturePngBytesAsync;
        ViewModel.ClearSignatureAction = ClearCanvas;

        // Prevent selecting future birth dates
        BirthDatePicker.MaxDate = DateTimeOffset.Now;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.LoadDataAsync();

            if (ViewModel.BirthDate.HasValue)
            {
                var dt = ViewModel.BirthDate.Value;
                BirthDatePicker.Date = new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, TimeSpan.Zero);
            }
        }
        catch (Exception ex)
        {
            App.LogAppMessage($"StudentGeneratorPage.Page_Loaded exception: {ex}");
        }
    }

    private void BirthDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        if (ViewModel == null) return;

        if (args.NewDate.HasValue)
        {
            var date = args.NewDate.Value.DateTime;
            ViewModel.BirthDate = new DateOnly(date.Year, date.Month, date.Day);
        }
        else
        {
            ViewModel.BirthDate = null;
        }
    }

    private void GenderMale_Checked(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.Gender = "Male";
        }
    }

    private void GenderFemale_Checked(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.Gender = "Female";
        }
    }

    private void SignatureCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel.IsLocked) return;

        var ptr = e.GetCurrentPoint(SignatureCanvas);
        _lastPoint = ptr.Position;
        _isDrawing = true;
        SignatureCanvas.CapturePointer(e.Pointer);
    }

    private void SignatureCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing || ViewModel.IsLocked) return;

        var currentPoint = e.GetCurrentPoint(SignatureCanvas).Position;

        var line = new Line
        {
            X1 = _lastPoint.X,
            Y1 = _lastPoint.Y,
            X2 = currentPoint.X,
            Y2 = currentPoint.Y,
            Stroke = new SolidColorBrush(ColorHelper.FromArgb(255, 31, 58, 95)), // Corporate navy accent #1F3A5F
            StrokeThickness = 2.5,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };

        SignatureCanvas.Children.Add(line);
        _hasStrokes = true;
        _lastPoint = currentPoint;
    }

    private void SignatureCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_isDrawing)
        {
            _isDrawing = false;
            SignatureCanvas.ReleasePointerCapture(e.Pointer);
        }
    }

    private void SignatureCanvas_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        _isDrawing = false;
    }

    private void ClearSignatureButton_Click(object sender, RoutedEventArgs e)
    {
        ClearCanvas();
    }

    private void ClearCanvas()
    {
        SignatureCanvas.Children.Clear();
        _hasStrokes = false;
    }

    public async Task<byte[]?> GetSignaturePngBytesAsync()
    {
        if (!_hasStrokes) return null;

        try
        {
            int width = (int)SignatureCanvas.ActualWidth;
            int height = (int)SignatureCanvas.ActualHeight;

            if (width <= 0) width = 400;
            if (height <= 0) height = 140;

            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(SignatureCanvas, width, height);

            var pixelBuffer = await rtb.GetPixelsAsync();
            byte[] pixelBytes = pixelBuffer.ToArray();

            using var memStream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memStream);

            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                (uint)rtb.PixelWidth,
                (uint)rtb.PixelHeight,
                96,
                96,
                pixelBytes);

            await encoder.FlushAsync();

            byte[] pngBytes = new byte[memStream.Size];
            using var reader = new DataReader(memStream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)memStream.Size);
            reader.ReadBytes(pngBytes);

            return pngBytes;
        }
        catch
        {
            return null;
        }
    }

    private async void OnUploadPhotoClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker();
            if (App.MainWindowInstance != null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using var stream = await file.OpenReadAsync();
                var bytes = new byte[stream.Size];
                using var reader = new DataReader(stream);
                await reader.LoadAsync((uint)stream.Size);
                reader.ReadBytes(bytes);
                await ViewModel.SetPhotoBytesAsync(bytes);
            }
        }
        catch (Exception ex)
        {
            ViewModel.PhotoError = $"Upload failed: {ex.Message}";
        }
    }

    private async void OnCapturePhotoClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var capture = new MediaCapture();
            await capture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video
            });

            var imgFormat = ImageEncodingProperties.CreateJpeg();
            using var stream = new InMemoryRandomAccessStream();
            await capture.CapturePhotoToStreamAsync(imgFormat, stream);
            stream.Seek(0);

            var bytes = new byte[stream.Size];
            using var reader = new DataReader(stream);
            await reader.LoadAsync((uint)stream.Size);
            reader.ReadBytes(bytes);

            await ViewModel.SetPhotoBytesAsync(bytes);
        }
        catch (Exception ex)
        {
            ViewModel.PhotoError = $"Camera not available or access denied ({ex.Message}). Please use the 'Upload Photo' button instead.";
        }
    }

    private void OnRemovePhotoClicked(object sender, RoutedEventArgs e)
    {
        ViewModel.RemovePhoto();
    }
}
