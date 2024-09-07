using ControlzEx.Theming;
using FIVStandard.Core.Statics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FIVStandard.Converters
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class TextToColorConverter : IValueConverter
    {
        private class ThemeItem
        {
            public string Name { get; set; }
            public Color PrimaryAccent { get; set; }
        }

        private readonly List<ThemeItem> ThemeItemList = [];

        public TextToColorConverter()
        {
            ReadOnlyObservableCollection<Theme> ThemeList = ThemeManager.Current.Themes;
            for (int i = 0; i < ThemeList.Count; i++)
            {
                ThemeItemList.Add(new ThemeItem()
                {
                    Name = ThemeList[i].Name.GetAfter('.'),
                    PrimaryAccent = ThemeList[i].PrimaryAccentColor,
                });
                //System.Diagnostics.Debug.WriteLine(ThemeItemList[i].Name);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            for (int i = 0; i < ThemeItemList.Count; i++)
            {
                if (string.Equals(ThemeItemList[i].Name, (string)value))
                {
                    return new SolidColorBrush(ThemeItemList[i].PrimaryAccent);
                }
            }

            return null;//something went wrong
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
