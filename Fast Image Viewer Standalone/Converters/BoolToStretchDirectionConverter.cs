using System;
using System.Globalization;
using System.Windows.Data;

namespace FIVStandard.Converters
{
    class BoolToStretchDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? System.Windows.Controls.StretchDirection.Both : System.Windows.Controls.StretchDirection.DownOnly;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
