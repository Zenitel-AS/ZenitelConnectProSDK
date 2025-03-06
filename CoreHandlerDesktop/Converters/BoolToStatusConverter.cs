using System;
using System.Globalization;
using System.Windows.Data;

namespace CoreHandlerDesktop.Converters
{
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Busy" : "Available";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == "Busy";
        }
    }
}
