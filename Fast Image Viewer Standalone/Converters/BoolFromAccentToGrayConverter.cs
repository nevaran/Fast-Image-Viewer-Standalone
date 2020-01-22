using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FIVStandard.Converters
{
    class BoolFromAccentToGrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c;
            if ((bool)value == true)
            {
                var accentColor = MahApps.Metro.ThemeManager.GetResourceFromAppStyle(Application.Current.MainWindow, "AccentColor");
                c = (Color)accentColor;
            }
            else
            {
                c = new Color
                {
                    A = 255,
                    R = 200,
                    G = 200,
                    B = 200
                };
            }
            SolidColorBrush brush = new SolidColorBrush(c);

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
