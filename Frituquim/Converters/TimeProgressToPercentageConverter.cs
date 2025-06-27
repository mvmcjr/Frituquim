using System;
using System.Globalization;
using System.Windows.Data;

namespace Frituquim.Converters
{
    public class TimeProgressToPercentageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && 
                values[0] is double currentSeconds && 
                values[1] is double totalSeconds && 
                totalSeconds > 0)
            {
                var percentage = (currentSeconds / totalSeconds) * 100;
                return Math.Min(100, Math.Max(0, percentage));
            }

            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
