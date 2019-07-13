using System;

namespace FIVStandard.Model
{
    public class SettingsManager : IDisposable
    {
        public struct Settings
        {
            public bool DarkTheme;
            public int ThemeAccent;
            public double ZoomSensitivity;
            public bool ImageStretched;
            public bool DownsizeImage;
            public int ShownLanguage;
        }
        public Settings setting = new Settings();

        public SettingsManager()
        {
            Properties.Settings.Default.SettingChanging += Default_SettingChanging;
        }

        public void Load()
        {
            setting.DarkTheme = Properties.Settings.Default.DarkTheme;
            setting.ThemeAccent = Properties.Settings.Default.ThemeAccent;
            setting.ZoomSensitivity = Properties.Settings.Default.ZoomSensitivity;
            setting.ImageStretched = Properties.Settings.Default.ImageStretched;
            setting.DownsizeImage = Properties.Settings.Default.DownsizeImage;
            setting.ShownLanguage = Properties.Settings.Default.ShownLanguage;
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }

        private void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            Save();
        }

        public void Dispose()
        {
            Properties.Settings.Default.SettingChanging -= Default_SettingChanging;
        }
    }
}
