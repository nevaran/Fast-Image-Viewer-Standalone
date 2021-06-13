using System;
using System.Globalization;
using System.Windows.Data;

namespace FIVStandard.Converters
{
    public class TimeSpanTickConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan ts = (TimeSpan)value;
            return ts.TotalMilliseconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = System.Convert.ToDouble(value);
            return TimeSpan.FromMilliseconds(d);
        }
    }
}