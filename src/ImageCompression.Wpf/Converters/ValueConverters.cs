namespace ImageCompression.Wpf.Converters;

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

/// <summary>
/// Converts a boolean condition to Visibility with support for inversion.
/// </summary>
public sealed class BooleanToVisibilityValueConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolVal = value is bool b && b;
        if (Invert)
        {
            boolVal = !boolVal;
        }

        return boolVal ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts a numeric PSNR delta to a color brush indicating which algorithm achieved higher fidelity.
/// </summary>
public sealed class DeltaToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush AdvantageDwtBrush = new(Color.FromRgb(56, 189, 248));
    private static readonly SolidColorBrush AdvantageDctBrush = new(Color.FromRgb(251, 146, 60));
    private static readonly SolidColorBrush NeutralBrush = new(Color.FromRgb(148, 163, 184));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double delta)
        {
            if (Math.Abs(delta) < 0.05)
            {
                return NeutralBrush;
            }

            return delta > 0 ? AdvantageDwtBrush : AdvantageDctBrush;
        }

        return NeutralBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
