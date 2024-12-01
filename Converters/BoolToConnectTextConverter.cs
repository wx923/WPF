using System.Globalization;
using System.Windows.Data;

namespace WpfApp.Converters;

public class BoolToConnectTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "断开连接" : "连接";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 