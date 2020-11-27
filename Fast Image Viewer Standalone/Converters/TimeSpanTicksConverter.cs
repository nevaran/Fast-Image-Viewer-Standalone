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
            return ts.Ticks;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long d = System.Convert.ToInt64(value);
            return new TimeSpan(d);
        }
    }
}