using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace IdGen.App.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility.Visible;
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is not true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is not true;
    }
}

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class BoolToSeverityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? InfoBarSeverity.Error : InfoBarSeverity.Success;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is int count && count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is not Visibility.Visible;
    }
}

public class IntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is int i && i > 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
        {
            return new SolidColorBrush(Colors.Transparent);
        }
        string clean = hex.Trim().TrimStart('#');
        byte a = 255;
        byte r = 255, g = 255, b = 255;

        try
        {
            if (clean.Length == 3)
            {
                r = System.Convert.ToByte(new string(clean[0], 2), 16);
                g = System.Convert.ToByte(new string(clean[1], 2), 16);
                b = System.Convert.ToByte(new string(clean[2], 2), 16);
            }
            else if (clean.Length == 6)
            {
                r = System.Convert.ToByte(clean.Substring(0, 2), 16);
                g = System.Convert.ToByte(clean.Substring(2, 2), 16);
                b = System.Convert.ToByte(clean.Substring(4, 2), 16);
            }
            else if (clean.Length == 8)
            {
                a = System.Convert.ToByte(clean.Substring(0, 2), 16);
                r = System.Convert.ToByte(clean.Substring(2, 2), 16);
                g = System.Convert.ToByte(clean.Substring(4, 2), 16);
                b = System.Convert.ToByte(clean.Substring(6, 2), 16);
            }
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }
        catch
        {
            return new SolidColorBrush(Colors.White);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
