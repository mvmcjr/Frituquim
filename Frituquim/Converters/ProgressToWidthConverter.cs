using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Frituquim.Converters
{
    public class ProgressToWidthConverter : IValueConverter
    {
        public static readonly ProgressToWidthConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress && parameter is FrameworkElement element)
            {
                var parentWidth = element.ActualWidth;
                return Math.Max(0, (progress / 100.0) * parentWidth);
            }

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
