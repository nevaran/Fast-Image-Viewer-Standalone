using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FIVStandard.Models
{
    public class SettingsManager : INotifyPropertyChanged
    {
        public struct Settings
        {
            public bool DarkTheme;
            public int ThemeAccent;
            public double ZoomSensitivity;
            public bool StretchedImage;
            public bool DownsizeImage;
            public int ShownLanguage;

            //key shortcuts
            public int goForwardKey;
            public int goBackwardKey;
            public int pauseKey;
            public int deleteKey;
            public int stretchImageKey;
            public int downsizeImageKey;
            public int exploreFileKey;
            public int copyToCLipboardKey;
        }
        public Settings settings = new Settings();

        #region Keys Properties
        private bool _shortcutButtonsOn = true;

        public bool ShortcutButtonsOn
        {
            get
            {
                return _shortcutButtonsOn;
            }
            set
            {
                _shortcutButtonsOn = value;
                OnPropertyChanged();
            }
        }

        private Key _goForwardKey = Key.Right;

        public Key GoForwardKey
        {
            get
            {
                return _goForwardKey;
            }
            set
            {
                _goForwardKey = value;
                OnPropertyChanged();
            }
        }

        private Key _goBackwardKey = Key.Left;

        public Key GoBackwardKey
        {
            get
            {
                return _goBackwardKey;
            }
            set
            {
                _goBackwardKey = value;
                OnPropertyChanged();
            }
        }

        private Key _pauseKey = Key.Space;

        public Key PauseKey
        {
            get
            {
                return _pauseKey;
            }
            set
            {
                _pauseKey = value;
                OnPropertyChanged();
            }
        }

        private Key _deleteKey = Key.Delete;

        public Key DeleteKey
        {
            get
            {
                return _deleteKey;
            }
            set
            {
                _deleteKey = value;
                OnPropertyChanged();
            }
        }

        private Key _stretchImageKey = Key.F;

        public Key StretchImageKey
        {
            get
            {
                return _stretchImageKey;
            }
            set
            {
                _stretchImageKey = value;
                OnPropertyChanged();
            }
        }

        private Key _downsizeImageKey = Key.D;

        public Key DownsizeImageKey
        {
            get
            {
                return _downsizeImageKey;
            }
            set
            {
                _downsizeImageKey = value;
                OnPropertyChanged();
            }
        }

        private Key _exploreFileKey = Key.E;

        public Key ExploreFileKey
        {
            get
            {
                return _exploreFileKey;
            }
            set
            {
                _exploreFileKey = value;
                OnPropertyChanged();
            }
        }

        private Key _copyToClipboardKey = Key.C;

        public Key CopyToClipboardKey
        {
            get
            {
                return _copyToClipboardKey;
            }
            set
            {
                _copyToClipboardKey = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public SettingsManager()
        {
            Properties.Settings.Default.SettingChanging += Default_SettingChanging;
        }

        public void Load()
        {
            var savedSettings = Properties.Settings.Default;

            settings.DarkTheme = savedSettings.DarkTheme;
            settings.ThemeAccent = savedSettings.ThemeAccent;
            settings.ZoomSensitivity = savedSettings.ZoomSensitivity;
            settings.StretchedImage = savedSettings.ImageStretched;
            settings.DownsizeImage = savedSettings.DownsizeImage;
            settings.ShownLanguage = savedSettings.ShownLanguage;

            settings.goForwardKey = savedSettings.GoForwardKey;
            settings.goBackwardKey = savedSettings.GoBackWardKey;
            settings.pauseKey = savedSettings.PauseKey;
            settings.deleteKey = savedSettings.DeleteKey;
            settings.stretchImageKey = savedSettings.StretchImageKey;
            settings.downsizeImageKey = savedSettings.DownsizeImageKey;
            settings.exploreFileKey = savedSettings.ExploreFileKey;
            settings.copyToCLipboardKey = savedSettings.CopyToClipboardKey;
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }

        public void ResetToDefault()
        {
            Properties.Settings.Default.Reset();

            Load();
        }

        private void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            Save();
        }

        public void Unload()
        {
            Properties.Settings.Default.SettingChanging -= Default_SettingChanging;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
