using System.Text.RegularExpressions;
using IdGen.Core.Models.Template;
using IdGen.Core.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace IdGen.App.Controls;

public sealed partial class TemplateElementView : UserControl
{
    public static readonly DependencyProperty ElementProperty =
        DependencyProperty.Register(nameof(Element), typeof(TemplateElementDefinition), typeof(TemplateElementView),
            new PropertyMetadata(null, (d, e) => ((TemplateElementView)d).RenderElement()));

    public static readonly DependencyProperty SampleTextProperty =
        DependencyProperty.Register(nameof(SampleText), typeof(string), typeof(TemplateElementView),
            new PropertyMetadata(string.Empty, (d, e) => ((TemplateElementView)d).RenderElement()));

    public static readonly DependencyProperty LogoSourceProperty =
        DependencyProperty.Register(nameof(LogoSource), typeof(ImageSource), typeof(TemplateElementView),
            new PropertyMetadata(null, (d, e) => ((TemplateElementView)d).RenderElement()));

    public static readonly DependencyProperty SignatureSourceProperty =
        DependencyProperty.Register(nameof(SignatureSource), typeof(ImageSource), typeof(TemplateElementView),
            new PropertyMetadata(null, (d, e) => ((TemplateElementView)d).RenderElement()));

    public static readonly DependencyProperty PhotoSourceProperty =
        DependencyProperty.Register(nameof(PhotoSource), typeof(ImageSource), typeof(TemplateElementView),
            new PropertyMetadata(null, (d, e) => ((TemplateElementView)d).RenderElement()));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(TemplateElementView),
            new PropertyMetadata(false, (d, e) => ((TemplateElementView)d).UpdateSelectionOutline()));

    public TemplateElementDefinition? Element
    {
        get => (TemplateElementDefinition?)GetValue(ElementProperty);
        set => SetValue(ElementProperty, value);
    }

    public string SampleText
    {
        get => (string)GetValue(SampleTextProperty);
        set => SetValue(SampleTextProperty, value);
    }

    public ImageSource? LogoSource
    {
        get => (ImageSource?)GetValue(LogoSourceProperty);
        set => SetValue(LogoSourceProperty, value);
    }

    public ImageSource? SignatureSource
    {
        get => (ImageSource?)GetValue(SignatureSourceProperty);
        set => SetValue(SignatureSourceProperty, value);
    }

    public ImageSource? PhotoSource
    {
        get => (ImageSource?)GetValue(PhotoSourceProperty);
        set => SetValue(PhotoSourceProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public TemplateElementView()
    {
        InitializeComponent();
        Loaded += (s, e) => RenderElement();
    }

    private void UpdateSelectionOutline()
    {
        SelectionBorder.BorderBrush = IsSelected
            ? new SolidColorBrush(Color.FromArgb(255, 37, 99, 235))
            : new SolidColorBrush(Colors.Transparent);
        SelectionBorder.BorderThickness = IsSelected ? new Thickness(2) : new Thickness(0);

        LockIndicator.Visibility = (Element != null && Element.Locked && IsSelected)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public void RenderElement()
    {
        ContentGrid.Children.Clear();
        if (Element == null) return;

        UpdateSelectionOutline();

        switch (Element.Type)
        {
            case "Rectangle":
                RenderRectangle();
                break;
            case "Line":
                RenderLine();
                break;
            case "Barcode":
                RenderBarcode();
                break;
            case "StaticText":
            case "DynamicText":
                RenderText();
                break;
            case "Logo":
                RenderLogo();
                break;
            case "Photo":
                RenderPhoto();
                break;
            case "Signature":
                RenderSignature();
                break;
        }
    }

    private void RenderRectangle()
    {
        if (Element == null) return;
        var border = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            CornerRadius = new CornerRadius(Math.Clamp(Element.CornerRadius, 0, 24)),
            BorderThickness = new Thickness(Math.Clamp(Element.BorderWidth, 0, 8))
        };

        if (!string.IsNullOrWhiteSpace(Element.BorderColor))
        {
            border.BorderBrush = ParseHexBrush(Element.BorderColor, 100);
        }

        if (!string.IsNullOrWhiteSpace(Element.FillColor))
        {
            border.Background = ParseHexBrush(Element.FillColor, Element.FillOpacity);
        }

        ContentGrid.Children.Add(border);
    }

    private void RenderLine()
    {
        if (Element == null) return;
        var lineBorder = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Height = Math.Clamp(Element.Thickness, 1, 16),
            Background = ParseHexBrush(Element.LineColor ?? "#000000", 100)
        };
        ContentGrid.Children.Add(lineBorder);
    }

    private void RenderBarcode()
    {
        if (Element == null) return;

        string studentNo = !string.IsNullOrWhiteSpace(SampleText) ? SampleText : "202110055";
        var result = Code128Encoder.Encode(studentNo, Element.IncludeQuietZone);

        var container = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        double textHeight = Element.ShowHumanReadable ? Math.Clamp(Element.Height * 0.22, 12, 18) : 0;
        double barsHeight = Math.Max(8.0, Element.Height - textHeight);

        container.RowDefinitions.Add(new RowDefinition { Height = new GridLength(barsHeight, GridUnitType.Pixel) });
        if (Element.ShowHumanReadable)
        {
            container.RowDefinitions.Add(new RowDefinition { Height = new GridLength(textHeight, GridUnitType.Pixel) });
        }

        // Barcode bars Canvas
        var barcodeCanvas = new Canvas
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Width = Element.Width,
            Height = barsHeight
        };

        double scaleX = Element.Width / result.TotalModules;
        var barBrush = ParseHexBrush(Element.BarcodeColor ?? "#000000", 100);

        foreach (var bar in result.Bars)
        {
            var rect = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Width = Math.Max(1.0, bar.Width * scaleX),
                Height = barsHeight,
                Fill = barBrush
            };
            Canvas.SetLeft(rect, bar.X * scaleX);
            Canvas.SetTop(rect, 0);
            barcodeCanvas.Children.Add(rect);
        }

        Grid.SetRow(barcodeCanvas, 0);
        container.Children.Add(barcodeCanvas);

        if (Element.ShowHumanReadable)
        {
            var text = new TextBlock
            {
                Text = studentNo,
                FontSize = Math.Clamp(textHeight * 0.72, 8, 12),
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = barBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0)
            };
            Grid.SetRow(text, 1);
            container.Children.Add(text);
        }

        ContentGrid.Children.Add(container);
    }

    private void RenderText()
    {
        if (Element == null) return;
        string text = !string.IsNullOrWhiteSpace(SampleText) ? SampleText : (Element.StaticText ?? "");
        if (string.Equals(Element.FieldKey, "semester", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(text))
        {
            text = TemplateConstants.FormatSemester(text);
        }

        var textBlock = new TextBlock
        {
            Text = text,
            FontFamily = new FontFamily(Element.FontFamily ?? "Segoe UI"),
            FontSize = Math.Clamp(Element.FontSize, 6, 72),
            FontWeight = Element.FontWeight == "Bold" ? Microsoft.UI.Text.FontWeights.Bold :
                         Element.FontWeight == "SemiBold" ? Microsoft.UI.Text.FontWeights.SemiBold :
                         Microsoft.UI.Text.FontWeights.Normal,
            FontStyle = Element.Italic ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal,
            Foreground = ParseHexBrush(Element.TextColor ?? "#000000", 100),
            TextAlignment = Element.TextAlign == "Center" ? TextAlignment.Center :
                            Element.TextAlign == "Right" ? TextAlignment.Right :
                            TextAlignment.Left,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };

        ContentGrid.Children.Add(textBlock);
    }

    private void RenderLogo()
    {
        var container = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        if (LogoSource != null)
        {
            var image = new Image
            {
                Source = LogoSource,
                Stretch = Stretch.Uniform
            };
            container.Children.Add(image);
        }
        else
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 100, 116, 139)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromArgb(40, 100, 116, 139))
            };
            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 2
            };
            stack.Children.Add(new FontIcon { Glyph = "\uEB9F", FontSize = 22, Foreground = new SolidColorBrush(Colors.SlateGray) });
            stack.Children.Add(new TextBlock { Text = "LOGO", FontSize = 10, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(Colors.SlateGray), HorizontalAlignment = HorizontalAlignment.Center });
            border.Child = stack;
            container.Children.Add(border);
        }

        ContentGrid.Children.Add(container);
    }

    private void RenderPhoto()
    {
        if (PhotoSource != null)
        {
            var photoBorder = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Color.FromArgb(160, 148, 163, 184)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
                Child = new Image
                {
                    Source = PhotoSource,
                    Stretch = Stretch.UniformToFill,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                }
            };
            ContentGrid.Children.Add(photoBorder);
            return;
        }

        var border = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            BorderBrush = new SolidColorBrush(Color.FromArgb(160, 148, 163, 184)),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(Color.FromArgb(60, 226, 232, 240)),
            CornerRadius = new CornerRadius(4)
        };

        var stack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 4
        };
        stack.Children.Add(new FontIcon { Glyph = "\uE77B", FontSize = 36, Foreground = new SolidColorBrush(Colors.SlateGray) });
        stack.Children.Add(new TextBlock { Text = "STUDENT PHOTO", FontSize = 11, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = new SolidColorBrush(Colors.SlateGray), HorizontalAlignment = HorizontalAlignment.Center });
        stack.Children.Add(new TextBlock { Text = "2x2 Placeholder", FontSize = 10, Foreground = new SolidColorBrush(Colors.Gray), HorizontalAlignment = HorizontalAlignment.Center });

        border.Child = stack;
        ContentGrid.Children.Add(border);
    }

    private void RenderSignature()
    {
        var container = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        if (SignatureSource != null)
        {
            var img = new Image
            {
                Source = SignatureSource,
                Stretch = Stretch.Uniform
            };
            container.Children.Add(img);
        }
        else
        {
            var border = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 148, 163, 184)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromArgb(30, 241, 245, 249))
            };

            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 2
            };
            stack.Children.Add(new FontIcon { Glyph = "\uEDC6", FontSize = 24, Foreground = new SolidColorBrush(Colors.DarkSlateGray) });
            stack.Children.Add(new TextBlock { Text = "SIGNATURE", FontSize = 10, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = new SolidColorBrush(Colors.DarkSlateGray) });

            border.Child = stack;
            container.Children.Add(border);
        }

        ContentGrid.Children.Add(container);
    }

    private static SolidColorBrush ParseHexBrush(string hex, double opacity = 100.0)
    {
        if (string.IsNullOrWhiteSpace(hex)) return new SolidColorBrush(Colors.Transparent);
        string clean = hex.Trim().TrimStart('#');

        byte a = 255;
        byte r = 0, g = 0, b = 0;

        if (clean.Length == 3)
        {
            r = Convert.ToByte(new string(clean[0], 2), 16);
            g = Convert.ToByte(new string(clean[1], 2), 16);
            b = Convert.ToByte(new string(clean[2], 2), 16);
        }
        else if (clean.Length == 6)
        {
            r = Convert.ToByte(clean.Substring(0, 2), 16);
            g = Convert.ToByte(clean.Substring(2, 2), 16);
            b = Convert.ToByte(clean.Substring(4, 2), 16);
        }
        else if (clean.Length == 8)
        {
            a = Convert.ToByte(clean.Substring(0, 2), 16);
            r = Convert.ToByte(clean.Substring(2, 2), 16);
            g = Convert.ToByte(clean.Substring(4, 2), 16);
            b = Convert.ToByte(clean.Substring(6, 2), 16);
        }

        double alphaFactor = Math.Clamp(opacity / 100.0, 0.0, 1.0);
        byte finalAlpha = (byte)(a * alphaFactor);

        return new SolidColorBrush(Color.FromArgb(finalAlpha, r, g, b));
    }
}
