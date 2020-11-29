using ControlzEx.Theming;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FIVStandard.Core
{
    public class SettingsManager : INotifyPropertyChanged, ISettings
    {
        public readonly MainWindow mainWindow;

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

        private Key _thumbnailListKey = Key.T;

        public Key ThumbnailListKey
        {
            get
            {
                return _thumbnailListKey;
            }
            set
            {
                _thumbnailListKey = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Settings Properties
        public readonly List<(string tag, string lang)> ShownLanguage = new List<(string tag, string lang)>()
        {
            ("en", "English (en)"),
            ("bg-BG", "Български (bg-BG)"),
            ("nl-NL", "Nederlands (nl-NL)"),
            ("pt-BR", "Portuguesa (pt-BR)"),
            ("se-SE", "Svenska (se-SE)"),
            ("da-DK", "Dansk (da-DK)"),
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
                OnPropertyChanged("CheckForUpdatesStartToggle");
                OnPropertyChanged("EnableThumbnailListToggle");

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

        private bool _autoupdateToggle = false;

        public bool AutoupdateToggle
        {
            get
            {
                return _autoupdateToggle;
            }
            set
            {
                _autoupdateToggle = value;
                OnPropertyChanged();

                OnAutoupdateToggle();
            }
        }

        private bool enableThumbnailListToggle = true;

        public bool EnableThumbnailListToggle
        {
            get
            {
                return enableThumbnailListToggle;
            }
            set
            {
                enableThumbnailListToggle = value;
                OnPropertyChanged();

                OnEnableThumbnailChanged();
            }
        }

        private int thumbnailSize = 80;

        public int ThumbnailSize
        {
            get
            {
                return thumbnailSize;
            }
            set
            {
                thumbnailSize = value;
                OnPropertyChanged();
                OnPropertyChanged("ThumbnailSizePlusText");
                OnPropertyChanged("ThumbnailSizeGifTag");

                OnThumbnailSizeChanged();
            }
        }

        public int ThumbnailSizePlusText
        {
            get
            {
                return (ThumbnailSize + 13);
            }
        }

        public int ThumbnailSizeGifTag
        {
            get
            {
                return (int)((float)ThumbnailSize / 3.2);
            }
        }

        private int thumbnailRes = 80;

        public int ThumbnailRes
        {
            get
            {
                return thumbnailRes;
            }
            set
            {
                thumbnailRes = value;
                OnPropertyChanged();

                OnThumbnailResChanged();
            }
        }

        public List<string> FilterActiveList { get; set; } = new List<string>();//".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico", ".webp"
        public string[] FilterActiveArray { get; set; } = null;

        private bool filterJpg = true;

        public bool FilterJpg
        {
            get
            {
                return filterJpg;
            }
            set
            {
                filterJpg = value;
                OnPropertyChanged();

                UpdateActiveFilterList();
            }
        }

        private bool filterJpeg = true;

        public bool FilterJpeg
        {
            get
            {
                return filterJpeg;
            }
            set
            {
                filterJpeg = value;
                OnPropertyChanged();

                UpdateActiveFilterList();
            }
        }

        private bool filterPng = true;

        public bool FilterPng
        {
            get
            {
                return filterPng;
            }
            set
            {
                filterPng = value;
                OnPropertyChanged();

                UpdateActiveFilterList();
            }
        }

        private bool filterGif = true;

        public bool FilterGif
        {
            get
            {
                return filterGif;
            }
            set
            {
                filterGif = value;
                OnPropertyChanged();

                UpdateActiveFilterList();
            }
        }

        private bool filterBmp = true;

        public bool FilterBmp
        {
            get
            {
                return filterBmp;
            }
            set
            {
                filterBmp = value;
                OnPropertyChanged();

                UpdateActiveFilterList();
            }
        }

        private bool filterTiff = true;

        public bool FilterTiff
        {
            get
            {
                return filterTiff;
            }
            set
            {
                filterTiff = value;
                OnPropertyChanged();

                UpdateActiveFilterList();
            }
        }

        private bool filterIco = true;

        public bool FilterIco
        {
            get
            {
                return filterIco;
            }
            set
            {
                filterIco = value;
                OnPropertyChanged();

                UpdateActiveFilterList();
            }
        }

        private bool filterSvg = true;

        public bool FilterSvg
        {
            get
            {
                return filterSvg;
            }
            set
            {
                filterSvg = value;
                OnPropertyChanged();

                UpdateActiveFilterList();
            }
        }

        private bool filterWebp = true;

        public bool FilterWebp
        {
            get
            {
                return filterWebp;
            }
            set
            {
                filterWebp = value;
                OnPropertyChanged();

                UpdateActiveFilterList();
            }
        }

        private bool filterWebm = true;

        public bool FilterWebm
        {
            get
            {
                return filterWebm;
            }
            set
            {
                filterWebm = value;
                OnPropertyChanged();

                UpdateActiveFilterList();
            }
        }

        public bool ReloadFolderFlag = false;//flag for confirming if the list of images needs to be reloaded (ie from changing what types to be displayed)
        #endregion

        public SettingsManager(MainWindow mw)
        {
            mainWindow = mw;

            //Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;

            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Save();
            }

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
            AutoupdateToggle = savs.AutoupdateToggle;
            EnableThumbnailListToggle = savs.EnableThumbnailList;
            ThumbnailSize = savs.ThumbnailSize;
            ThumbnailRes = savs.ThumbnailRes;

            GoForwardKey = (Key)savs.GoForwardKey;
            GoBackwardKey = (Key)savs.GoBackWardKey;
            PauseKey = (Key)savs.PauseKey;
            DeleteKey = (Key)savs.DeleteKey;
            StretchImageKey = (Key)savs.StretchImageKey;
            DownsizeImageKey = (Key)savs.DownsizeImageKey;
            ExploreFileKey = (Key)savs.ExploreFileKey;
            CopyImageToClipboardKey = (Key)savs.CopyToClipboardKey;
            CutFileToClipboardKey = (Key)savs.CutToClipboardKey;
            ThumbnailListKey = (Key)savs.ThumbnailListKey;

            FilterJpg = savs.FilterJpg;
            FilterJpeg = savs.FilterJpeg;
            FilterPng = savs.FilterPng;
            FilterGif = savs.FilterGif;
            FilterBmp = savs.FilterBmp;
            FilterTiff = savs.FilterTiff;
            FilterIco = savs.FilterIco;
            FilterSvg = savs.FilterSvg;
            FilterWebp = savs.FilterWebp;
            FilterWebm = savs.FilterWebm;
        }

        public static void Save()
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
            savs.ThumbnailListKey = (int)ThumbnailListKey;
        }

        public void ResetToDefault()
        {
            Properties.Settings.Default.Reset();

            Load();
        }

        /*private void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Save();
        }

        public void Unload()
        {
            Properties.Settings.Default.PropertyChanged -= Default_PropertyChanged;
        }*/

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
            string theme;
            if (DarkModeToggle)
            {
                theme = "Dark";
                //ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[ThemeAccentDropIndex]), ThemeManager.GetAppTheme("BaseDark"));
            }
            else
            {
                theme = "Light";
            }
            ThemeManager.Current.ChangeTheme(mainWindow, $"{theme}.{ThemeAccents[ThemeAccentDropIndex]}");
        }

        private void OnLanguageChanged()
        {
            Properties.Settings.Default.ShownLanguage = ShownLanguageDropIndex;

            Localization.TranslationSource.Instance.CurrentCulture = CultureInfo.GetCultureInfo(ShownLanguage[ShownLanguageDropIndex].tag);
        }

        private void OnStretchSwitch()
        {
            Properties.Settings.Default.ImageStretched = StretchImageToggle;
        }

        private void OnDownsizeSwitch()
        {
            Properties.Settings.Default.DownsizeImage = DownsizeImageToggle;

            if (mainWindow.ImagesData.Count > 0)
            {
                mainWindow.ImageSource = Tools.LoadImage(mainWindow.ActivePath, mainWindow.ImgWidth, mainWindow.ImgHeight);
            }
        }

        private void OnZoomSensitivitySlider()
        {
            Properties.Settings.Default.ZoomSensitivity = ZoomSensitivity;
        }

        private void OnEnableThumbnailChanged()
        {
            Properties.Settings.Default.EnableThumbnailList = EnableThumbnailListToggle;
        }

        private void OnThumbnailSizeChanged()
        {
            Properties.Settings.Default.ThumbnailSize = ThumbnailSize;
        }

        private void OnThumbnailResChanged()
        {
            Properties.Settings.Default.ThumbnailRes = ThumbnailRes;
            mainWindow.ThumbnailResSlider_ValueChanged();
        }

        private void OnCheckForUpdatesStartToggle()
        {
            Properties.Settings.Default.CheckUpdateAtStart = CheckForUpdatesStartToggle;
        }

        private void OnAutoupdateToggle()
        {
            Properties.Settings.Default.AutoupdateToggle = AutoupdateToggle;
        }

        public void ClearActiveFilterList()
        {
            FilterActiveList.Clear();
        }

        public void AddActiveFilter(string newFilterElement)
        {
            FilterActiveList.Add(newFilterElement);
        }

        public void UpdateActiveFilterList()
        {
            ClearActiveFilterList();

            if (FilterJpg)
                AddActiveFilter(".jpg");
            if (FilterJpeg)
                AddActiveFilter(".jpeg");
            if (FilterPng)
                AddActiveFilter(".png");
            if (FilterGif)
                AddActiveFilter(".gif");
            if (FilterBmp)
                AddActiveFilter(".bmp");
            if (FilterTiff)
                AddActiveFilter(".tiff");
            if (FilterIco)
                AddActiveFilter(".ico");
            if (FilterSvg)
                AddActiveFilter(".svg");
            if (FilterWebp)
                AddActiveFilter(".webp");
            if (FilterWebm)
                AddActiveFilter(".webm");

            FilterActiveArray = FilterActiveList.ToArray();

            if (mainWindow.ProgramLoaded == false) return;//fixes crash

            UpdateAllTypesProperties();

            ReloadFolderFlag = true;
        }

        public void UpdateAllTypesProperties()
        {
            var savs = Properties.Settings.Default;//saved settings shortcut

            savs.FilterJpg = FilterJpg;
            savs.FilterJpeg = FilterJpeg;
            savs.FilterPng = FilterPng;
            savs.FilterGif = FilterGif;
            savs.FilterBmp = FilterBmp;
            savs.FilterTiff = FilterTiff;
            savs.FilterIco = FilterIco;
            savs.FilterSvg = FilterSvg;
            savs.FilterWebp = FilterWebp;
            savs.FilterWebm = FilterWebm;
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
