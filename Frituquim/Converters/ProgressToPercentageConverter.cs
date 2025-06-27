using System;
using System.Globalization;
using System.Windows.Data;

namespace Frituquim.Converters
{
    public class ProgressToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                return $"{progress:P1}"; // Format as percentage with 1 decimal place
            }

            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
