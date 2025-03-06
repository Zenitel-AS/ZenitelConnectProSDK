using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CoreHandlerDesktop.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBusy)
            {
                return isBusy ? Brushes.Red : Brushes.Green; // Red = Busy, Green = Available
            }
            return Brushes.Gray; // Default color if value is not a boolean
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush == Brushes.Red; // Convert back to bool (true if Red, false otherwise)
            }
            return false;
        }
    }
}
