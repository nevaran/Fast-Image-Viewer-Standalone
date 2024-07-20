using ControlzEx.Theming;
using FIVStandard.Model;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FIVStandard.Core
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public sealed class SettingsManager
    {
        private readonly MainWindow mainWindow;

        public SettingsJson JSettings { get; set; }

        public bool ReloadFolderFlag = false;//flag for confirming if the list of images needs to be reloaded (ie from changing what types of files are to be displayed)

        private readonly string settingsPath = "";

        public SettingsManager(MainWindow mw)
        {
            mainWindow = mw;

            settingsPath = @$"{AppDomain.CurrentDomain.BaseDirectory}\settings.json";

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
            LanguageChanged();
            UpdateActiveFilterList();

            //TODO: temporary test code
            mainWindow.Width = JSettings.WindowWidth;
            mainWindow.Height = JSettings.WindowHeight;
            mainWindow.WindowState = JSettings.WindowState;
        }

        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = false,
        };

        public void Save()
        {
            if (JSettings is null) return;

            JsonSerializerOptions options = jsonSerializerOptions;
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
            JSettings.FilterAll = true;

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

        private void ThemeSwitch()
        {
            ChangeTheme();
        }

        private void AccentChanged()
        {
            ChangeTheme();//since theme also is rooted with accent
        }

        private void ChangeTheme()
        {
            string theme = JSettings.DarkModeToggle ? "Dark" : "Light";

            _ = ThemeManager.Current.ChangeTheme(Application.Current, $"{theme}.{JSettings.ThemeAccents[JSettings.ThemeAccentDropIndex]}");
        }

        private void LanguageChanged()
        {
            Localization.TranslationSource.Instance.CurrentCulture = CultureInfo.GetCultureInfo(JSettings.ShownLanguage[JSettings.ShownLanguageDropIndex].tag);
        }

        private async Task DownsizeSwitch()
        {
            if (mainWindow.ImagesData.Count > 0 && mainWindow.ImageItem.FileType == FileMediaType.Image)//reload image if its supported by the downsize feature (not animated/media)
            {
                //mainWindow.ImageSource = await Tools.LoadImage(mainWindow.ActivePath, mainWindow.ImgWidth, mainWindow.ImgHeight, mainWindow);
                await mainWindow.ChangeImageAsync(0, false, false);
            }
        }

        private void ThumbnailResChanged()
        {
            if (!mainWindow.ThumbnailSlider_DragStarted)
                mainWindow.ReloadAllThumbnailsAsync();
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
            {
                AddActiveFilter(".tiff");
                AddActiveFilter(".tif");//include alternative name format
            }
            if (JSettings.FilterIco)
                AddActiveFilter(".ico");
            if (JSettings.FilterSvg)
                AddActiveFilter(".svg");
            if (JSettings.FilterWebp)
                AddActiveFilter(".webp");
            if (JSettings.FilterWebm)
                AddActiveFilter(".webm");

            if (!JSettings.FilterJpg || 
                !JSettings.FilterJpeg || 
                !JSettings.FilterPng || 
                !JSettings.FilterGif || 
                !JSettings.FilterBmp || 
                !JSettings.FilterTiff || 
                !JSettings.FilterIco || 
                !JSettings.FilterSvg || 
                !JSettings.FilterWebp || 
                !JSettings.FilterWebm)
                JSettings.FilterAll = false;
            else
                JSettings.FilterAll = true;

            JSettings.FilterActiveArray = [.. JSettings.FilterActiveList];

            if (mainWindow.programLoaded == false) return;//fixes crash

            ReloadFolderFlag = true;
        }

        /// <summary>
        /// Use this if there is any method needing to be called in this class when a property from the SettingsJson is changed
        /// </summary>
        private async void SettingsJson_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(JSettings.ShownLanguageDropIndex):
                    LanguageChanged();
                    break;
                case nameof(JSettings.DarkModeToggle):
                    ThemeSwitch();
                    break;
                case nameof(JSettings.ThemeAccentDropIndex):
                    AccentChanged();
                    break;
                case nameof(JSettings.DownsizeImageToggle):
                    await DownsizeSwitch();
                    break;
                case nameof(JSettings.ThumbnailRes):
                    ThumbnailResChanged();
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
    }
}
