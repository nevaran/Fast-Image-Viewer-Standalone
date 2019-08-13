using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FIVStandard.Converters
{
    public class TextToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (string)value;
            var c = new Color
            {
                A = 255
            };
            if (color == "Emerald")
            {
                c.R = 7;
                c.G = 117;
                c.B = 7;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else if (color == "Cobalt")
            {
                c.R = 7;
                c.G = 71;
                c.B = 198;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else if (color == "Amber")
            {
                c.R = 199;
                c.G = 137;
                c.B = 15;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else if (color == "Steel")
            {
                c.R = 87;
                c.G = 101;
                c.B = 115;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else if (color == "Mauve")
            {
                c.R = 101;
                c.G = 84;
                c.B = 117;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else if (color == "Taupe")
            {
                c.R = 115;
                c.G = 104;
                c.B = 69;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else
            {
                return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
