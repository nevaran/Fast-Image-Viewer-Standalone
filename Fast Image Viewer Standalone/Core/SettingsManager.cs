using ControlzEx.Theming;
using FIVStandard.Model;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace FIVStandard.Core
{
    public class SettingsManager : INotifyPropertyChanged
    {
        public readonly MainWindow mainWindow;

        public SettingsJson JSettings { get; set; }

        public bool ReloadFolderFlag = false;//flag for confirming if the list of images needs to be reloaded (ie from changing what types of files are to be displayed)

        private readonly string settingsPath = "";

        public SettingsManager(MainWindow mw)
        {
            mainWindow = mw;

            settingsPath = @$"{mainWindow.StartupPath}\settings.json";

            Load();

            JSettings.PropertyChanged += SettingsJson_PropertyChanged;
        }

        public void Load()
        {
            if (File.Exists(settingsPath))
            {
                string jsonString = File.ReadAllText(settingsPath);
                JSettings = JsonSerializer.Deserialize<SettingsJson>(jsonString);
            }
            else
            {
                JSettings = new SettingsJson();
            }

            //Call all essential methods connected to the properties in the JSettings class
            ChangeTheme();
            OnLanguageChanged();
            UpdateActiveFilterList();

            //TODO: temporary test code
            mainWindow.Width = JSettings.WindowWidth;
            mainWindow.Height = JSettings.WindowHeight;
            mainWindow.WindowState = JSettings.WindowState;
        }

        public void Save()
        {
            if (JSettings is null) return;

            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
            };
            string jsonString = JsonSerializer.Serialize(JSettings, options);
            File.WriteAllText(settingsPath, jsonString);

            //Properties.Settings.Default.Save();
        }

        public void ResetToDefault()
        {
            if (JSettings is null) return;

            JSettings.ShownLanguageDropIndex = 0;
            JSettings.DarkModeToggle = true;
            JSettings.ThemeAccentDropIndex = 4;
            JSettings.StretchImageToggle = true;
            JSettings.DownsizeImageToggle = false;
            JSettings.ZoomSensitivity = 0.3;
            JSettings.CheckForUpdatesStartToggle = true;
            JSettings.AutoupdateToggle = false;
            JSettings.EnableThumbnailListToggle = true;
            JSettings.ThumbnailSize = 120;
            JSettings.ThumbnailRes = 120;

            JSettings.GoForwardKey = Key.Right;
            JSettings.GoBackwardKey = Key.Left;
            JSettings.PauseKey = Key.Space;
            JSettings.DeleteKey = Key.Delete;
            JSettings.StretchImageKey = Key.F;
            JSettings.DownsizeImageKey = Key.D;
            JSettings.ExploreFileKey = Key.E;
            JSettings.CopyImageToClipboardKey = Key.C;
            JSettings.CutFileToClipboardKey = Key.X;
            JSettings.ThumbnailListKey = Key.T;

            JSettings.FilterJpg = true;
            JSettings.FilterJpeg = true;
            JSettings.FilterPng = true;
            JSettings.FilterGif = true;
            JSettings.FilterBmp = true;
            JSettings.FilterTiff = true;
            JSettings.FilterIco = true;
            JSettings.FilterSvg = true;
            JSettings.FilterWebp = true;
            JSettings.FilterWebm = true;

            JSettings.MediaMuted = false;
            JSettings.MediaVolume = 0.5;

            JSettings.WindowWidth = 800;
            JSettings.WindowHeight = 600;
            JSettings.WindowState = WindowState.Maximized;
            //TODO: find better solution
            mainWindow.Width = JSettings.WindowWidth;
            mainWindow.Height = JSettings.WindowHeight;
            mainWindow.WindowState = JSettings.WindowState;
        }

        private void OnThemeSwitch()
        {
            ChangeTheme();
        }

        private void OnAccentChanged()
        {
            ChangeTheme();//since theme also is rooted with accent
        }

        private void ChangeTheme()
        {
            string theme;
            if (JSettings.DarkModeToggle)
            {
                theme = "Dark";
            }
            else
            {
                theme = "Light";
            }
            ThemeManager.Current.ChangeTheme(Application.Current, $"{theme}.{JSettings.ThemeAccents[JSettings.ThemeAccentDropIndex]}");
        }

        private void OnLanguageChanged()
        {
            Localization.TranslationSource.Instance.CurrentCulture = CultureInfo.GetCultureInfo(JSettings.ShownLanguage[JSettings.ShownLanguageDropIndex].tag);
        }

        private void OnDownsizeSwitch()
        {
            if (mainWindow.ImagesData.Count > 0 && !mainWindow.ImageItem.IsAnimated)
            {
                mainWindow.ImageSource = Tools.LoadImage(mainWindow.ActivePath, mainWindow.ImgWidth, mainWindow.ImgHeight);
            }
        }

        private void OnThumbnailResChanged()
        {
            mainWindow.ThumbnailResSlider_ValueChanged();
        }

        public void ClearActiveFilterList()
        {
            JSettings.FilterActiveList.Clear();
        }

        public void AddActiveFilter(string newFilterElement)
        {
            JSettings.FilterActiveList.Add(newFilterElement);
        }

        public void UpdateActiveFilterList()
        {
            ClearActiveFilterList();

            if (JSettings.FilterJpg)
                AddActiveFilter(".jpg");
            if (JSettings.FilterJpeg)
                AddActiveFilter(".jpeg");
            if (JSettings.FilterPng)
                AddActiveFilter(".png");
            if (JSettings.FilterGif)
                AddActiveFilter(".gif");
            if (JSettings.FilterBmp)
                AddActiveFilter(".bmp");
            if (JSettings.FilterTiff)
                AddActiveFilter(".tiff");
            if (JSettings.FilterIco)
                AddActiveFilter(".ico");
            if (JSettings.FilterSvg)
                AddActiveFilter(".svg");
            if (JSettings.FilterWebp)
                AddActiveFilter(".webp");
            if (JSettings.FilterWebm)
                AddActiveFilter(".webm");

            JSettings.FilterActiveArray = JSettings.FilterActiveList.ToArray();

            if (mainWindow.ProgramLoaded == false) return;//fixes crash

            ReloadFolderFlag = true;
        }

        /// <summary>
        /// Use this if there is any method needing to be called in this class when a property from the SettingsJson is changed
        /// </summary>
        private void SettingsJson_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(JSettings.ShownLanguageDropIndex):
                    OnLanguageChanged();
                    break;
                case nameof(JSettings.DarkModeToggle):
                    OnThemeSwitch();
                    break;
                case nameof(JSettings.ThemeAccentDropIndex):
                    OnAccentChanged();
                    break;
                case nameof(JSettings.DownsizeImageToggle):
                    OnDownsizeSwitch();
                    break;
                case nameof(JSettings.ThumbnailRes):
                    OnThumbnailResChanged();
                    break;
                case nameof(JSettings.FilterJpg):
                    UpdateActiveFilterList();
                    break;
                case nameof(JSettings.FilterJpeg):
                    UpdateActiveFilterList();
                    break;
                case nameof(JSettings.FilterPng):
                    UpdateActiveFilterList();
                    break;
                case nameof(JSettings.FilterGif):
                    UpdateActiveFilterList();
                    break;
                case nameof(JSettings.FilterBmp):
                    UpdateActiveFilterList();
                    break;
                case nameof(JSettings.FilterTiff):
                    UpdateActiveFilterList();
                    break;
                case nameof(JSettings.FilterIco):
                    UpdateActiveFilterList();
                    break;
                case nameof(JSettings.FilterSvg):
                    UpdateActiveFilterList();
                    break;
                case nameof(JSettings.FilterWebp):
                    UpdateActiveFilterList();
                    break;
                case nameof(JSettings.FilterWebm):
                    UpdateActiveFilterList();
                    break;
            }
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
