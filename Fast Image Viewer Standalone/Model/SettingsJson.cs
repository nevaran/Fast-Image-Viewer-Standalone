using FIVStandard.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FIVStandard.Model
{
    public class SettingsJson : INotifyPropertyChanged, ISettings
    {
        #region Settings Properties
        private int shownLanguageDropIndex = 0;

        public int ShownLanguageDropIndex
        {
            get
            {
                return shownLanguageDropIndex;
            }
            set
            {
                shownLanguageDropIndex = value;
                OnPropertyChanged();
            }
        }

        private bool darkModeToggle = true;

        public bool DarkModeToggle
        {
            get
            {
                return darkModeToggle;
            }
            set
            {
                darkModeToggle = value;
                OnPropertyChanged();
            }
        }

        //required to be here because of ISettings
        public List<string> ThemeAccents { get; } = new List<string> { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };

        private int themeAccentDropIndex = 0;

        public int ThemeAccentDropIndex
        {
            get
            {
                return themeAccentDropIndex;
            }
            set
            {
                themeAccentDropIndex = value;
                OnPropertyChanged();
                OnPropertyChanged("CheckForUpdatesStartToggle");
                OnPropertyChanged("EnableThumbnailListToggle");
            }
        }

        private bool stretchImageToggle = true;

        public bool StretchImageToggle
        {
            get
            {
                return stretchImageToggle;
            }
            set
            {
                stretchImageToggle = value;
                OnPropertyChanged();
            }
        }

        private bool downsizeImageToggle = false;

        public bool DownsizeImageToggle
        {
            get
            {
                return downsizeImageToggle;
            }
            set
            {
                downsizeImageToggle = value;
                OnPropertyChanged();
            }
        }

        private double zoomSensitivity = 0.3;

        public double ZoomSensitivity
        {
            get
            {
                return zoomSensitivity;
            }
            set
            {
                zoomSensitivity = value;
                OnPropertyChanged();
                OnPropertyChanged("ZoomSensitivityString");
            }
        }

        public string ZoomSensitivityString
        {
            get
            {
                return ZoomSensitivity.ToString("F2");
            }
        }

        private bool checkForUpdatesStartToggle = true;

        public bool CheckForUpdatesStartToggle
        {
            get
            {
                return checkForUpdatesStartToggle;
            }
            set
            {
                checkForUpdatesStartToggle = value;
                OnPropertyChanged();
            }
        }

        private bool autoupdateToggle = false;

        public bool AutoupdateToggle
        {
            get
            {
                return autoupdateToggle;
            }
            set
            {
                autoupdateToggle = value;
                OnPropertyChanged();
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
                return (int)((float)ThumbnailSize / 3);
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
            }
        }

        #region File Types
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
            }
        }
        #endregion

        private bool mediaMuted = false;

        public bool MediaMuted
        {
            get
            {
                return mediaMuted;
            }
            set
            {
                mediaMuted = value;
                OnPropertyChanged();
                OnPropertyChanged("VolumeIcon");
            }
        }

        private double mediaVolume = 0.5;

        public double MediaVolume
        {
            get
            {
                return mediaVolume;
            }
            set
            {
                mediaVolume = value;
                OnPropertyChanged();
                OnPropertyChanged("VolumeIcon");
            }
        }

        public MahApps.Metro.IconPacks.PackIconBoxIconsKind VolumeIcon
        {
            get
            {
                if (MediaMuted) return MahApps.Metro.IconPacks.PackIconBoxIconsKind.RegularVolumeMute;

                if (MediaVolume > 0.8)
                    return MahApps.Metro.IconPacks.PackIconBoxIconsKind.RegularVolumeFull;
                else if (MediaVolume > 0.0)
                    return MahApps.Metro.IconPacks.PackIconBoxIconsKind.RegularVolumeLow;

                return MahApps.Metro.IconPacks.PackIconBoxIconsKind.RegularVolume;
            }
        }

        private int windowWidth = 800;

        public int WindowWidth
        {
            get
            {
                return windowWidth;
            }
            set
            {
                windowWidth = value;
            }
        }

        private int windowHeight = 600;

        public int WindowHeight
        {
            get
            {
                return windowHeight;
            }
            set
            {
                windowHeight = value;
            }
        }

        private WindowState windowState = WindowState.Maximized;

        public WindowState WindowState
        {
            get
            {
                return windowState;
            }
            set
            {
                windowState = value;
            }
        }
        #endregion

        #region Keys Properties
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}