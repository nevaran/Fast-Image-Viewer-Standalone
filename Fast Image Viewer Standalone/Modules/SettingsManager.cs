using Gu.Localization;
using MahApps.Metro;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FIVStandard.Modules
{
    public class SettingsManager : INotifyPropertyChanged
    {
        private readonly MainWindow mainWindow;

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

        private Key _copyImageToClipboardKey = Key.C;

        public Key CopyImageToClipboardKey
        {
            get
            {
                return _copyImageToClipboardKey;
            }
            set
            {
                _copyImageToClipboardKey = value;
                OnPropertyChanged();
            }
        }

        private Key _cutFileToClipboardKey = Key.X;

        public Key CutFileToClipboardKey
        {
            get
            {
                return _cutFileToClipboardKey;
            }
            set
            {
                _cutFileToClipboardKey = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Settings Properties
        public readonly List<(string tag, string lang)> ShownLanguage = new List<(string tag, string lang)>()
        {
            ("en", "English (en)"),
            ("bg-BG", "Български (bg-BG)"),
            ("nl-NL", "Dutch (nl-BE/nl-NL)"),
            ("pt-BR", "Portuguese (pt-BR)"),
            ("se-SE", "Swedish (se-SE)"),
        };

        private int _shownLanguageDropIndex = 0;

        public int ShownLanguageDropIndex
        {
            get
            {
                return _shownLanguageDropIndex;
            }
            set
            {
                _shownLanguageDropIndex = value;
                OnPropertyChanged();

                OnLanguageChanged();
            }
        }

        public string[] GetLanguageString
        {
            get
            {
                return ShownLanguage.Select(x => x.lang).ToArray();
            }
        }

        private bool _darkModeToggle = true;

        public bool DarkModeToggle
        {
            get
            {
                return _darkModeToggle;
            }
            set
            {
                _darkModeToggle = value;
                OnPropertyChanged();

                OnThemeSwitch();
            }
        }

        //public List<string> ThemeAccents { get; } = new List<string> { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };
        public List<string> ThemeAccents { get; } = new List<string> { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };

        private int _themeAccentDropIndex = 0;

        public int ThemeAccentDropIndex
        {
            get
            {
                return _themeAccentDropIndex;
            }
            set
            {
                _themeAccentDropIndex = value;
                OnPropertyChanged();

                OnAccentChanged();
            }
        }

        private bool _stretchImageToggle = true;

        public bool StretchImageToggle
        {
            get
            {
                return _stretchImageToggle;
            }
            set
            {
                _stretchImageToggle = value;
                OnPropertyChanged();

                OnStretchSwitch();
            }
        }

        private bool _downsizeImageToggle = false;

        public bool DownsizeImageToggle
        {
            get
            {
                return _downsizeImageToggle;
            }
            set
            {
                _downsizeImageToggle = value;
                OnPropertyChanged();

                OnDownsizeSwitch();
            }
        }

        private double _zoomSensitivity = 1.0;

        public double ZoomSensitivity
        {
            get
            {
                return _zoomSensitivity;
            }
            set
            {
                _zoomSensitivity = value;
                OnPropertyChanged();
                OnPropertyChanged("ZoomSensitivityString");

                OnZoomSensitivitySlider();
            }
        }

        public string ZoomSensitivityString
        {
            get
            {
                return ZoomSensitivity.ToString("F2");
            }
        }

        private bool _checkForUpdatesStartToggle = true;

        public bool CheckForUpdatesStartToggle
        {
            get
            {
                return _checkForUpdatesStartToggle;
            }
            set
            {
                _checkForUpdatesStartToggle = value;
                OnPropertyChanged();

                OnCheckForUpdatesStartToggle();
            }
        }
        #endregion

        public SettingsManager(MainWindow mw)
        {
            mainWindow = mw;

            Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;

            //ThemeManager.AddAccent("HackerTheme", new Uri("pack://application:,,,/MahAppsMetroThemesSample;component/CustomAccents/HackerTheme.xaml"));
        }

        public void Load()
        {
            var savs = Properties.Settings.Default;//saved settings shortcut

            ShownLanguageDropIndex = savs.ShownLanguage;
            DarkModeToggle = savs.DarkTheme;
            ThemeAccentDropIndex = savs.ThemeAccent;
            StretchImageToggle = savs.ImageStretched;
            DownsizeImageToggle = savs.DownsizeImage;
            ZoomSensitivity = savs.ZoomSensitivity;
            CheckForUpdatesStartToggle = savs.CheckUpdateAtStart;

            GoForwardKey = (Key)savs.GoForwardKey;
            GoBackwardKey = (Key)savs.GoBackWardKey;
            PauseKey = (Key)savs.PauseKey;
            DeleteKey = (Key)savs.DeleteKey;
            StretchImageKey = (Key)savs.StretchImageKey;
            DownsizeImageKey = (Key)savs.DownsizeImageKey;
            ExploreFileKey = (Key)savs.ExploreFileKey;
            CopyImageToClipboardKey = (Key)savs.CopyToClipboardKey;
            CutFileToClipboardKey = (Key)savs.CutToClipboardKey;
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }

        public void UpdateAllKeysProperties()
        {
            var savs = Properties.Settings.Default;//saved settings shortcut

            savs.GoForwardKey = (int)GoForwardKey;
            savs.GoBackWardKey = (int)GoBackwardKey;
            savs.PauseKey = (int)PauseKey;
            savs.DeleteKey = (int)DeleteKey;
            savs.StretchImageKey = (int)StretchImageKey;
            savs.DownsizeImageKey = (int)DownsizeImageKey;
            savs.ExploreFileKey = (int)ExploreFileKey;
            savs.CopyToClipboardKey = (int)CopyImageToClipboardKey;
            savs.CutToClipboardKey = (int)CutFileToClipboardKey;
        }

        public void ResetToDefault()
        {
            Properties.Settings.Default.Reset();

            Load();
        }

        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Save();
        }

        public void Unload()
        {
            Properties.Settings.Default.PropertyChanged -= Default_PropertyChanged;
        }

        private void OnThemeSwitch()
        {
            Properties.Settings.Default.DarkTheme = DarkModeToggle;
            ChangeTheme();
        }

        private void OnAccentChanged()
        {
            Properties.Settings.Default.ThemeAccent = ThemeAccentDropIndex;
            ChangeTheme();//since theme also is rooted with accent
        }

        private void ChangeTheme()
        {
            if (DarkModeToggle)
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[ThemeAccentDropIndex]), ThemeManager.GetAppTheme("BaseDark"));
            }
            else
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[ThemeAccentDropIndex]), ThemeManager.GetAppTheme("BaseLight"));
            }

            //Properties.Settings.Default.Save();
        }

        private void OnLanguageChanged()
        {
            Properties.Settings.Default.ShownLanguage = ShownLanguageDropIndex;
            //Properties.Settings.Default.Save();

            Translator.Culture = CultureInfo.GetCultureInfo(ShownLanguage[ShownLanguageDropIndex].tag);
        }

        private void OnStretchSwitch()
        {
            if (StretchImageToggle)
            {
                mainWindow.MediaView.StretchDirection = System.Windows.Controls.StretchDirection.Both;
                mainWindow.PictureView.StretchDirection = System.Windows.Controls.StretchDirection.Both;
            }
            else
            {
                mainWindow.MediaView.StretchDirection = System.Windows.Controls.StretchDirection.DownOnly;
                mainWindow.PictureView.StretchDirection = System.Windows.Controls.StretchDirection.DownOnly;
            }

            Properties.Settings.Default.ImageStretched = StretchImageToggle;
            //Properties.Settings.Default.Save();
        }

        private void OnDownsizeSwitch()
        {
            Properties.Settings.Default.DownsizeImage = DownsizeImageToggle;

            if (mainWindow.ImagesFound.Count > 0)
                mainWindow.ImageSource = mainWindow.LoadImage(mainWindow.ActivePath);

            //Properties.Settings.Default.Save();
        }

        private void OnZoomSensitivitySlider()
        {
            Properties.Settings.Default.ZoomSensitivity = ZoomSensitivity;

            //Properties.Settings.Default.Save();
        }

        private void OnCheckForUpdatesStartToggle()
        {
            Properties.Settings.Default.CheckUpdateAtStart = CheckForUpdatesStartToggle;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
