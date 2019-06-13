using FIVStandard.Backend;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FIVStandard
{
    public partial class MainView : MetroWindow
    {
<<<<<<< HEAD
        public MainView()
        {
            //InitializeComponent();
=======
        private BaseViewModel baseVM;

        public int ImageIndex { get; set; } = 0;

        private bool isPaused = false;

        public int ImgWidth { get; set; } = 0;
        public int ImgHeight { get; set; } = 0;

        //private string startupPath;//program startup path

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        public bool IsDeletingFile { get; private set; } = false;
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
                baseVM.SetAndNotify(ref _themeAccentDropIndex, value);
                //NotifyOfPropertyChange(() => ThemeAccentDropIndex);
                OnAccentChanged();
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
                NotifyOfPropertyChange(() => DarkModeToggle);
                OnThemeSwitch();
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
                NotifyOfPropertyChange(() => StretchImageToggle);
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
                NotifyOfPropertyChange(() => DownsizeImageToggle);
                OnDownsizeChanged();
            }
        }

        private string _windowTitle = "FIV";

        public string WindowTitle
        {
            get
            {
                return _windowTitle;
            }
            set
            {
                _windowTitle = value;
                NotifyOfPropertyChange(() => WindowTitle);
            }
        }

        private double _zoomSensitivity = 0.2;

        public double ZoomSensitivity
        {
            get
            {
                return _zoomSensitivity;
            }
            set
            {
                _zoomSensitivity = value;
                NotifyOfPropertyChange(() => ZoomSensitivity);
                NotifyOfPropertyChange(() => ZoomSensitivityString);
                OnZoomSensitivityChanged();
            }
        }

        public string ZoomSensitivityString
        {
            get
            {
                return $"{ZoomSensitivity}";
            }
        }

        private Uri _mediaSource = null;

        public Uri MediaSource
        {
            get
            {
                return _mediaSource;
            }
            set
            {
                _mediaSource = value;
                NotifyOfPropertyChange(() => MediaSource);
            }
        }

        private BitmapImage _imageSource = null;

        public BitmapImage ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                NotifyOfPropertyChange(() => ImageSource);
            }
        }

        private Visibility _borderImgVisibility = Visibility.Visible;

        public Visibility BorderImgVisible
        {
            get
            {
                return _borderImgVisibility;
            }
            set
            {
                _borderImgVisibility = value;
                NotifyOfPropertyChange(() => BorderImgVisible);
            }
        }

        private Visibility _borderMediaVisible = Visibility.Hidden;

        public Visibility BorderMediaVisible
        {
            get
            {
                return _borderMediaVisible;
            }
            set
            {
                _borderMediaVisible = value;
                NotifyOfPropertyChange(() => BorderMediaVisible);
            }
        }

        private double _borderImageWidth;

        public double BorderImageWidth
        {
            get
            {
                return _borderImageWidth;
            }
            set
            {
                _borderImageWidth = value;
                NotifyOfPropertyChange(() => BorderImageWidth);
            }
        }

        private double _borderImageHeight;

        public double BorderImageHeight
        {
            get
            {
                return _borderImageHeight;
            }
            set
            {
                _borderImageHeight = value;
                NotifyOfPropertyChange(() => BorderImageHeight);
            }
        }

        private string _imageInfo;

        public string ImageInfo
        {
            get
            {
                return _imageInfo;
            }
            set
            {
                _imageInfo = value;
                NotifyOfPropertyChange(() => ImageInfo);
            }
        }

        private bool _settingsFlyout = false;

        public bool SettingsFlyout
        {
            get
            {
                return _settingsFlyout;
            }
            set
            {
                _settingsFlyout = value;
                NotifyOfPropertyChange(() => SettingsFlyout);
            }
        }

        private bool _helpFlyout = false;

        public bool HelpFlyout
        {
            get
            {
                return _helpFlyout;
            }
            set
            {
                _helpFlyout = value;
                NotifyOfPropertyChange(() => HelpFlyout);
            }
        }

        private StretchDirection _imageViewStretchDir = StretchDirection.DownOnly;

        public StretchDirection ImageViewStretchDir
        {
            get
            {
                return _imageViewStretchDir;
            }
            set
            {
                _imageViewStretchDir = value;
                NotifyOfPropertyChange(() => ImageViewStretchDir);
            }
        }

        private readonly FileLoaderModel fileLoader;

        public ICommand SettingsCommand { get; set; }
        public ICommand DonateCommand { get; set; }
        public ICommand HelpCommand { get; set; }
        public ICommand AccentSplitCommand { get; set; }
        public ICommand OpenFileLocationCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public MainView()
        {
            InitializeComponent();

            baseVM = new BaseViewModel();

            SettingsCommand = new RelayCommand(OnSettingsClick);
            DonateCommand = new RelayCommand(OnDonateClick);
            HelpCommand = new RelayCommand(OnHelpClick);
            AccentSplitCommand = new RelayCommand(OnAccentClick);
            OpenFileLocationCommand = new RelayCommand(OnOpenFileLocation);
            DeleteCommand = new RelayCommand(OnDeleteClick);

            fileLoader = new FileLoaderModel(this);

            LoadAllSettings();

            //AppWindow = this;//used for debugging ZoomBorder
        }

        public void OnAppLoaded(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 0)//get startup path
            {
                //startupPath = Path.GetDirectoryName(args[0]);

#if DEBUG
                //string path = "D:\\Google Drive\\temp\\qmrns28.gif";
                //fileLoader.OpenNewFile(path);
#endif
            }

            if (args.Length > 1)
            {
                fileLoader.OpenNewFile(args[1]);
                SetTitleInformation();
            }
        }

        public void SetTitleInformation()
        {
            WindowTitle = $"[{ImageIndex + 1}/{fileLoader.ImagesFound.Count}] {Path.GetFileName(fileLoader.ImagesFound[ImageIndex])}";
        }

        /// <summary>
        /// Clear all saved paths and clean media view and finally cleanup memory
        /// </summary>
        private void ClearAllMedia()
        {
            fileLoader.ImagesFound.Clear();
            MediaSource = null;
            ImageSource = null;
            WindowTitle = "FIV";

            //GC.Collect();
        }

        public void OnClipOpened(object sender, RoutedEventArgs e)
        {
            if (fileLoader.ImagesFound.Count == 0) return;

            using (var imageStream = File.OpenRead(fileLoader.ImagesFound[ImageIndex]))
            {
                var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                ImgWidth = decoder.Frames[0].PixelWidth;
                ImgHeight = decoder.Frames[0].PixelHeight;

                ImageInfo = $"Image: {ImgWidth}x{ImgHeight} (View: {BorderImageWidth}x{BorderImageHeight})";
            }

            /*if (MediaView.NaturalDuration.HasTimeSpan)//used for videos (avi mp4 etc.)
            {
                TimeSpan ts = MediaView.NaturalDuration.TimeSpan;

                if(ts.TotalSeconds > 0)
                {
                    ImageInfoText.Text += "\nDuration ";

                    if (ts.Hours > 0)
                        ImageInfoText.Text += $"{ts.Hours}H ";

                    if (ts.Minutes > 0)
                        ImageInfoText.Text += $"{ts.Minutes}m ";

                    if (ts.Seconds > 0)
                        ImageInfoText.Text += $"{ts.Seconds}s";
                }
            }*/
        }

        public void OnClipEnded(object sender, RoutedEventArgs e)
        {
            //MediaView.Position = new TimeSpan(0, 0, 1);//TODO
            //MediaView.Play();//TODO
            MainView.mainView.MediaView.Position = new TimeSpan(0, 0, 1);
            MainView.mainView.MediaView.Play();
        }

        private void ChangeImage(int jump)
        {
            if (fileLoader.ImagesFound.Count == 0)//no more images in the folder - go back to default null
            {
                ClearAllMedia();
                return;
            }

            ImageIndex += jump;
            if (ImageIndex < 0) ImageIndex = fileLoader.ImagesFound.Count - 1;
            if (ImageIndex >= fileLoader.ImagesFound.Count) ImageIndex = 0;

            if (!FileSystem.FileExists(fileLoader.ImagesFound[ImageIndex]))//keep moving onward until we find an existing file
            {
                //refresh the file lists in the directory
                //GetDirectoryFiles(Path.GetDirectoryName(imagesFound[imageIndex]));
                //FindIndexInFiles(imagesFound[imageIndex]);

                //remove nonexistent file from list - if there are more than 1
                if (fileLoader.ImagesFound.Count > 1)
                {
                    fileLoader.ImagesFound.RemoveAt(ImageIndex);
                    SetTitleInformation();
                }

                ChangeImage(jump);

                return;
            }

            fileLoader.NewUri(fileLoader.ImagesFound[ImageIndex]);

            SetTitleInformation();
        }

        private void TogglePause()
        {
            if (fileLoader.IsAnimated)
            {
                if (isPaused)
                {
                    MainView.mainView.MediaView.Play();
                    //MediaView.Play();//TODO
                    isPaused = false;
                }
                else
                {
                    MainView.mainView.MediaView.Pause();
                    ///MediaView.Pause();//TODO
                    isPaused = true;
                }
            }
        }

        public void ImageChanged()
        {
            if (fileLoader.IsAnimated)
            {
                isPaused = false;

                MainView.mainView.MediaView.Play();
                MainView.mainView.MediaBorder.Reset();
                //MediaView.Play();//TODO
                //border.Reset();//TODO
            }
            else
            {
                MainView.mainView.ImageBorder.Reset();
                //borderImg.Reset();//TODO
            }
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (IsDeletingFile) return;

            if (e.Key == Key.Right)
            {
                ChangeImage(1);//go forward
            }
            if (e.Key == Key.Left)
            {
                ChangeImage(-1);//go back
            }

            if (e.Key == Key.Space)
            {
                TogglePause();
            }

            if (e.Key == Key.Delete && fileLoader.ImagesFound.Count > 0)
            {
                DeleteToRecycle(fileLoader.ImagesFound[ImageIndex]);
            }

            if (e.Key == Key.F)
            {
                StretchImageToggle = !StretchImageToggle;
            }

            if (e.Key == Key.E)
            {
                ExploreFile();
            }
        }

        public void OnClick_Next(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile) return;

            ChangeImage(1);//go forward
        }

        public void OnClick_Prev(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile) return;

            ChangeImage(-1);//go back
        }

        public void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsDeletingFile) return;

            if (e.ChangedButton == MouseButton.XButton1)
            {
                ChangeImage(-1);//go back
            }
            if (e.ChangedButton == MouseButton.XButton2)
            {
                ChangeImage(1);//go forward
            }
        }

        public void OnOpenBrowseImage(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile) return;

            Nullable<bool> result = fileLoader.DoOpenFileDialog.ShowDialog();
            if (result == true)
            {
                fileLoader.OpenNewFile(fileLoader.DoOpenFileDialog.FileName);
            }
            else
            {
                //cancelled dialog
            }

            //GC.Collect();
        }

        public void OnSettingsClick()
        {
            HelpFlyout = false;
            SettingsFlyout = !SettingsFlyout;
        }

        public void OnHelpClick()
        {
            SettingsFlyout = false;
            HelpFlyout = !HelpFlyout;
        }

        public void OnCloseFlyoutsClick(object sender, MouseButtonEventArgs e)
        {
            SettingsFlyout = false;
            HelpFlyout = false;
        }

        public void OnDonateClick()
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6ZXTCHB3JXL4Q&source=url");
            Process.Start(sInfo);
        }

        private void LoadAllSettings()
        {
            //Theme
            DarkModeToggle = Properties.Settings.Default.DarkTheme;
            ChangeTheme(Properties.Settings.Default.ThemeAccent);
            //Accent
            //ThemeAccentDrop.ItemsSource = themeAccents;//init for theme list
            ThemeAccentDropIndex = Properties.Settings.Default.ThemeAccent;
            //Image Stretch
            StretchImageToggle = Properties.Settings.Default.ImageStretched;
            //Downsize Images
            DownsizeImageToggle = Properties.Settings.Default.DownsizeImage;
            //Zoom Sensitivity
            ZoomSensitivity = Properties.Settings.Default.ZoomSensitivity;
        }

        public void OnThemeSwitch()
        {
            Properties.Settings.Default.DarkTheme = DarkModeToggle;
            ChangeTheme(Properties.Settings.Default.ThemeAccent);
        }

        private void ChangeTheme(int themeIndex)
        {
            if (DarkModeToggle)
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[themeIndex]), ThemeManager.GetAppTheme("BaseDark"));
            }
            else
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[themeIndex]), ThemeManager.GetAppTheme("BaseLight"));
            }

            Properties.Settings.Default.Save();
        }

        public void OnAccentChanged()
        {
            Properties.Settings.Default.ThemeAccent = ThemeAccentDropIndex;
            ChangeTheme(ThemeAccentDropIndex);//since theme also is rooted with accent
        }

        public void OnAccentClick()
        {
            Properties.Settings.Default.ThemeAccent++;

            if (Properties.Settings.Default.ThemeAccent > ThemeAccents.Count - 1)
                Properties.Settings.Default.ThemeAccent = 0;

            ThemeAccentDropIndex = Properties.Settings.Default.ThemeAccent;
        }

        public void OnStretchSwitch()
        {
            Properties.Settings.Default.ImageStretched = StretchImageToggle;
            if (StretchImageToggle)
            {
                ImageViewStretchDir = StretchDirection.Both;
            }
            else
            {
                ImageViewStretchDir = StretchDirection.DownOnly;
            }

            Properties.Settings.Default.Save();
        }

        public void OnOpenFileLocation()
        {
            ExploreFile();
        }

        public void ExploreFile()
        {
            try
            {
                if (File.Exists(fileLoader.ImagesFound[ImageIndex]))
                {
                    //Clean up file path so it can be navigated OK
                    Process.Start("explorer.exe", string.Format("/select,\"{0}\"", Path.GetFullPath(fileLoader.ImagesFound[ImageIndex])));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void OnDeleteClick()
        {
            if (IsDeletingFile) return;

            DeleteToRecycle(fileLoader.ImagesFound[ImageIndex]);
        }

        private async void DeleteToRecycle(string path)
        {
            await Task.Run(() =>
            {
                try
                {
                    IsDeletingFile = true;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WindowTitle = "Deleting " + Path.GetFileName(path) + "...";
                    });

                    if (FileSystem.FileExists(path))
                    {
                        FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);

                        //remove removed item from list
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            fileLoader.ImagesFound.RemoveAt(ImageIndex);
                            ChangeImage(-1);//go back to a previous file after deletion
                        });
                    }
                    else
                    {
                        MessageBox.Show("File not found: " + path);
                    }

                    IsDeletingFile = false;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\nIndex: " + ImageIndex);
                }
            }
            );
        }

        public void OnDownsizeChanged()//TODO: not downsizing
        {
            Properties.Settings.Default.DownsizeImage = DownsizeImageToggle;
            if (fileLoader.ImagesFound.Count > 0)
            {
                ImageSource = fileLoader.LoadImage(new Uri(fileLoader.ImagesFound[ImageIndex], UriKind.Absolute), DownsizeImageToggle);
            }

            Properties.Settings.Default.Save();
        }

        public void OnZoomSensitivityChanged()
        {
            Properties.Settings.Default.ZoomSensitivity = ZoomSensitivity;
            Properties.Settings.Default.Save();
>>>>>>> parent of 003606e... wth is going on
        }
    }
}
