using System.Windows.Media;

namespace AivoraPOS.App.Converters;

public sealed class StockLevelToBrushConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var key = value as string ?? string.Empty;
        return key switch
        {
            "Low" => new SolidColorBrush(Color.FromRgb(198, 40, 40)),
            "Warning" => new SolidColorBrush(Color.FromRgb(245, 124, 0)),
            _ => new SolidColorBrush(Color.FromRgb(46, 125, 50))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
        throw new NotSupportedException();
}
