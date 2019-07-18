using System;

namespace FIVStandard.Models
{
    public class SettingsManager
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
        public Settings settings = new Settings();

        public SettingsManager()
        {
            Properties.Settings.Default.SettingChanging += Default_SettingChanging;
        }

        public void Load()
        {
            settings.DarkTheme = Properties.Settings.Default.DarkTheme;
            settings.ThemeAccent = Properties.Settings.Default.ThemeAccent;
            settings.ZoomSensitivity = Properties.Settings.Default.ZoomSensitivity;
            settings.ImageStretched = Properties.Settings.Default.ImageStretched;
            settings.DownsizeImage = Properties.Settings.Default.DownsizeImage;
            settings.ShownLanguage = Properties.Settings.Default.ShownLanguage;
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }

        private void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            Save();
        }

        public void Unload()
        {
            Properties.Settings.Default.SettingChanging -= Default_SettingChanging;
        }
    }
}
