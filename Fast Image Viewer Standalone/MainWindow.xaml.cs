using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using System.Drawing;
using System.Globalization;
using Gu.Localization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using FIVStandard.Models;
using System.Collections.Specialized;

namespace FIVStandard
{
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private int ImageIndex { get; set; } = 0;
        private List<string> ImagesFound { get; set; } = new List<string>();

        private bool IsAnimated { get; set; } = false;
        private bool IsPaused { get; set; } = false;

        #region Image Properties
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private int _imgWidth = 0;
        public int ImgWidth
        {
            get
            {
                return _imgWidth;
            }
            set
            {
                _imgWidth = value;
                OnPropertyChanged();
                OnPropertyChanged("ImgResolution");
            }
        }

        private int _imgHeight = 0;
        public int ImgHeight
        {
            get
            {
                return _imgHeight;
            }
            set
            {
                _imgHeight = value;
                OnPropertyChanged();
                OnPropertyChanged("ImgResolution");
            }
        }

        public string ImgResolution
        {
            get
            {
                if (_imgWidth == 0 || _imgHeight == 0)
                    return "owo";
                else
                    return $"{_imgWidth}x{_imgHeight}";
            }
        }

        public Rotation ImageRotation { get; set; } = Rotation.Rotate0;
        #endregion

        #region Settings Properties
        private readonly List<(string tag, string lang)> ShownLanguage = new List<(string tag, string lang)>()
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
        #endregion

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

        public UpdateCheck _appUpdater;//NOTE: do not use anywhere

        public UpdateCheck AppUpdater
        {
            get
            {
                if (_appUpdater == null)
                {
                    _appUpdater = new UpdateCheck(this);
                }

                return _appUpdater;
            }
            set
            {
                if (_appUpdater == null)
                {
                    _appUpdater = new UpdateCheck(this);
                }

                _appUpdater = value;
            }
        }

        public CopyFileToClipboard ToClipboard;

        //private string StartupPath;//program startup path

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        private readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif"/*, ".tiff"*/, ".bmp"/*, ".svg"*/, ".ico"/*, ".mp4", ".avi" */, ".JPG", ".JPEG", ".GIF", ".BMP", ".ICO", ".PNG" };//TODO: doesnt work: tiff svg
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Images (*.JPG, *.JPEG, *.PNG, *.GIF, *.BMP, *ICO)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.ICO"/* + "|All files (*.*)|*.*" */};

        private System.Windows.Controls.Button editingButton = null;

        private bool IsDeletingFile { get; set; } = false;

        private string ActiveFile { get; set; } = "";//file name + extension
        private string ActiveFolder { get; set; } = "";//directory
        private string ActivePath { get; set; } = "";//directory + file name + extension

        private readonly FileSystemWatcher fsw = new FileSystemWatcher()
        {
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite
            , IncludeSubdirectories = false
        };

        public readonly Notifier notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(5),
                maximumNotificationCount: MaximumNotificationCount.FromCount(4));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        public MainWindow()
        {
            InitializeComponent();

            ToClipboard = new CopyFileToClipboard();

            //create new watcher events for used directory
            fsw.Changed += Fsw_Updated;
            fsw.Deleted += Fsw_Updated;
            fsw.Created += Fsw_Updated;
            fsw.Renamed += Fsw_Updated;

            DataContext = this;

            LoadAllSettings();

            //AppWindow = this;//used for debugging ZoomBorder
        }

        private void OnAppLoaded(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 0)//get startup path
            {
                //StartupPath = Path.GetDirectoryName(args[0]);

#if DEBUG
                string path = @"D:\Google Drive\temp\alltypes\4.gif";

                OpenNewFile(path);
#endif
            }

            if (args.Length > 1)
            {
                OpenNewFile(args[1]);
            }

            /*notifier.ShowInformation("");
            notifier.ShowSuccess("");
            notifier.ShowWarning("");
            notifier.ShowError("");*/
        }

        public void OpenNewFile(string path)
        {
            if (IsDeletingFile || _shortcutButtonsOn == false) return;

            ActiveFile = Path.GetFileName(path);
            ActiveFolder = Path.GetDirectoryName(path);
            ActivePath = path;

            fsw.Path = ActiveFolder;
            fsw.EnableRaisingEvents = true;//File Watcher is enabled/disabled

            GetDirectoryFiles(ActiveFolder);

            FindIndexInFiles(ActiveFile);
            SetTitleInformation();

            NewUri(path);
        }

        private void Fsw_Updated(object sender, FileSystemEventArgs e)//TODO: create more sophisticated updating where it doesnt load the whole directory again
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string cultureChangeType;
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        cultureChangeType = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.ChangedWatcher));
                        break;
                    case WatcherChangeTypes.Created:
                        cultureChangeType = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.CreatedWatcher));
                        break;
                    case WatcherChangeTypes.Deleted:
                        cultureChangeType = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.DeletedWatcher));
                        break;
                    case WatcherChangeTypes.Renamed:
                        cultureChangeType = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.RenamedWatcher));
                        break;
                    default:
                        cultureChangeType = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.AllWatcher));
                        break;
                }
                notifier.ShowInformation($"{cultureChangeType} \"{e.Name}\"");

                GetDirectoryFiles(ActiveFolder);

                if (ImagesFound.Count < 1)
                {
                    ClearAllMedia();
                    return;
                }

                FindIndexInFiles(ActiveFile);
                //SetTitleInformation();

                ChangeImage(0);
            });
        }

        private void GetDirectoryFiles(string searchFolder)
        {
            ImagesFound.Clear();
            List<string> filesFound = new List<string>();

            //filesFound.AddRange(Directory.GetFiles(searchFolder, "*.*", SearchOption.TopDirectoryOnly));
            //filesFound.AddRange(Directory.EnumerateFiles(searchFolder).OrderBy(filename => filename));
            //filesFound.OrderBy(p => p.Substring(0)).ToList();//probably doesnt work
            filesFound.AddRange(Directory.EnumerateFiles(searchFolder));

            int c = filesFound.Count;
            for (int i = 0; i < c; i++)
            {
                if (filters.Any(Path.GetExtension(filesFound[i]).Contains))
                {
                    filesFound[i] = Path.GetFileName(filesFound[i]);
                    ImagesFound.Add(filesFound[i]);
                }
            }

            ImagesFound.Sort(new NameComparer());
        }

        private void FindIndexInFiles(string openedFile)
        {
            int L = ImagesFound.Count;
            for (int i = 0; i < L; i++)
            {
                if(openedFile == ImagesFound[i])
                {
                    ImageIndex = i;
                    ActiveFile = ImagesFound[ImageIndex];
                    ActivePath = Path.Combine(ActiveFolder, ActiveFile);
                    //MessageBox.Show(imagesFound.Count + " | " + imageIndex);//DEBUG
                    break;
                }
            }
        }

        private void SetTitleInformation()
        {
            Title = $"[{ImageIndex + 1}/{ImagesFound.Count}] {ImagesFound[ImageIndex]}";//TODO: fix crash when directory of active path is deleted
        }

        /// <summary>
        /// Clear all saved paths and clean media view and finally cleanup memory
        /// </summary>
        private void ClearAllMedia()
        {
            ImagesFound.Clear();
            MediaSource = null;
            ImageSource = null;
            ImgWidth = 0;
            ImgHeight = 0;
            Title = "FIV";

            //GC.Collect();
        }

        private void OnClipEnded(object sender, RoutedEventArgs e)
        {
            MediaView.Position = new TimeSpan(0, 0, 1);
            MediaView.Play();
        }

        private void ChangeImage(int jump)
        {
            if (ImagesFound.Count == 0)//no more images in the folder - go back to default null
            {
                ClearAllMedia();
                return;
            }

            ImageIndex += jump;
            if (ImageIndex < 0) ImageIndex = ImagesFound.Count - 1;
            if (ImageIndex >= ImagesFound.Count) ImageIndex = 0;

            if (!FileSystem.FileExists(Path.Combine(ActiveFolder, ImagesFound[ImageIndex])))//keep moving onward until we find an existing file
            {
                //refresh the file lists in the directory
                //GetDirectoryFiles(Path.GetDirectoryName(imagesFound[imageIndex]));
                //FindIndexInFiles(imagesFound[imageIndex]);

                //remove nonexistent file from list - if there are more than 1
                if (ImagesFound.Count > 1)
                {
                    ImagesFound.RemoveAt(ImageIndex);
                    SetTitleInformation();
                }

                ChangeImage(jump);

                return;
            }

            ActiveFile = ImagesFound[ImageIndex];
            ActivePath = Path.Combine(ActiveFolder, ActiveFile);

            NewUri(ActivePath);

            SetTitleInformation();
        }

        private void TogglePause()
        {
            /*controller = ImageBehavior.GetAnimationController(MainImage);

            if (!isAnimated) return;

            if (controller.IsPaused)
                controller.Play();
            else
                controller.Pause();*/

            if (IsAnimated)
            {
                if (IsPaused)
                {
                    MediaView.Play();
                    IsPaused = false;
                }
                else
                {
                    MediaView.Pause();
                    IsPaused = true;
                }
            }
        }

        private void NewUri(string path)
        {
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();//DEBUG
            stopwatch.Start();//DEBUG
#endif
            string pathext = Path.GetExtension(path);
            if (pathext == ".gif"/* || pathext == ".mp4" || pathext == ".avi"*/)
            {
                IsAnimated = true;
            }
            else
                IsAnimated = false;

            Uri uri = new Uri(path, UriKind.Absolute);

            if (IsAnimated)
            {
                borderImg.Visibility = Visibility.Hidden;
                border.Visibility = Visibility.Visible;

                ImageSource = null;
                MediaSource = uri;

                IsPaused = false;

                MediaView.Play();
                border.Reset();
            }
            else
            {
                borderImg.Visibility = Visibility.Visible;
                border.Visibility = Visibility.Hidden;

                GetImageInformation(ActivePath);

                //MediaView?.Close();
                MediaSource = null;
                ImageSource = LoadImage(path);

                borderImg.Reset();
            }
#if DEBUG
            stopwatch.Stop();//DEBUG
            notifier.ShowError($"NewUri time: {stopwatch.ElapsedMilliseconds}ms");//DEBUG
#endif

            //GC.Collect();
        }

        private BitmapImage LoadImage(string path)
        {
            BitmapImage imgTemp = new BitmapImage();
            FileStream stream = File.OpenRead(path);
            imgTemp.BeginInit();
            imgTemp.CacheOption = BitmapCacheOption.OnLoad;//TODO: remove this so it loads faster - needs to make workaround for deleting file from file lockup
            //imgTemp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;//TODO: remove this so it loads faster - needs to make workaround for deleting file
            imgTemp.StreamSource = stream;

            if (_downsizeImageToggle)
            {
                Rect r = WpfScreen.GetScreenFrom(this).ScreenBounds;
                /*if (ImgWidth > borderImg.ActualWidth)
                    imgTemp.DecodePixelWidth = (int)r.Width;
                else if (ImgHeight > borderImg.ActualHeight)
                    imgTemp.DecodePixelHeight = (int)r.Height;*/

                imgTemp.DecodePixelWidth = (int)(ImgWidth * ScaleToBox(ImgWidth, (int)r.Width, ImgHeight, (int)r.Height));
            }
            if (ImageRotation != Rotation.Rotate0)
                imgTemp.Rotation = ImageRotation;

            imgTemp.EndInit();
            imgTemp.Freeze();
            stream.Close();
            stream.Dispose();

            return imgTemp;
        }

        private double ScaleToBox(double w, double sw, double h, double sh)
        {
            double scaleWidth = sw / w;
            double scaleHeight = sh / h;

            double scale = Math.Min(scaleWidth, scaleHeight);

            return scale;
        }

        private void OnDonateClick(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6ZXTCHB3JXL4Q&source=url");
            Process.Start(sInfo);
        }

        private void LoadAllSettings()
        {
            //Language
            ShownLanguageDropIndex = Properties.Settings.Default.ShownLanguage;
            //Theme
            DarkModeToggle = Properties.Settings.Default.DarkTheme;
            //Accent
            //ThemeAccentDrop.ItemsSource = ThemeAccents;//init for theme list
            ThemeAccentDropIndex = Properties.Settings.Default.ThemeAccent;
            //Image Stretch
            StretchImageToggle = Properties.Settings.Default.ImageStretched;
            //Downsize Images
            DownsizeImageToggle = Properties.Settings.Default.DownsizeImage;
            //Zoom Sensitivity
            ZoomSensitivity = Properties.Settings.Default.ZoomSensitivity;

            //ChangeTheme(Properties.Settings.Default.ThemeAccent);
            //ChangeAccent();//not needed since we calling ChangeTheme in there
        }

        private void OnThemeSwitch()
        {
            Properties.Settings.Default.DarkTheme = _darkModeToggle;
            ChangeTheme();
        }

        private void ChangeTheme()
        {
            if (_darkModeToggle)
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[_themeAccentDropIndex]), ThemeManager.GetAppTheme("BaseDark"));
            }
            else
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[_themeAccentDropIndex]), ThemeManager.GetAppTheme("BaseLight"));
            }

            Properties.Settings.Default.Save();
        }

        private void OnAccentChanged()
        {
            Properties.Settings.Default.ThemeAccent = _themeAccentDropIndex;
            ChangeTheme();//since theme also is rooted with accent
        }

        private void OnLanguageChanged()
        {
            Properties.Settings.Default.ShownLanguage = _shownLanguageDropIndex;
            Properties.Settings.Default.Save();

            Translator.Culture = CultureInfo.GetCultureInfo(ShownLanguage[_shownLanguageDropIndex].tag);
        }

        private void OnStretchSwitch()
        {
            if (_stretchImageToggle)
            {
                MediaView.StretchDirection = System.Windows.Controls.StretchDirection.Both;
                PictureView.StretchDirection = System.Windows.Controls.StretchDirection.Both;
            }
            else
            {
                MediaView.StretchDirection = System.Windows.Controls.StretchDirection.DownOnly;
                PictureView.StretchDirection = System.Windows.Controls.StretchDirection.DownOnly;
            }

            Properties.Settings.Default.ImageStretched = _stretchImageToggle;
            Properties.Settings.Default.Save();
        }

        public void ExploreFile()
        {
            try
            {
                if (File.Exists(ActivePath))
                {
                    //Clean up file path so it can be navigated OK
                    Process.Start("explorer.exe", string.Format("/select,\"{0}\"", Path.GetFullPath(ActivePath)));
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                notifier.ShowInformation(e.Message);
            }
        }

        private Task DeleteToRecycleAsync(string path)
        {
            if (!File.Exists(path)) return Task.CompletedTask;

            return Task.Run(() =>
            {
                try
                {
                    IsDeletingFile = true;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Title = "Deleting " + ActiveFile + "...";
                    });

                    if (FileSystem.FileExists(path))
                    {
                        FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);

                        //remove deleted item from list
                        /*Application.Current.Dispatcher.Invoke(() =>
                        {
                            ImagesFound.RemoveAt(ImageIndex);
                            ChangeImage(-1);//go back to a previous file after deletion
                            //SetTitleInformation();
                        });*/
                    }
                    else
                    {
                        //MessageBox.Show("File not found: " + path);
                        string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.FileNotFoundMsg));
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            notifier.ShowWarning($"{cultureTranslated}: {path}");
                        });
                    }

                    IsDeletingFile = false;
                }
                catch (Exception e)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //MessageBox.Show(e.Message + "\nIndex: " + ImageIndex);
                        notifier.ShowError(e.Message + "\nIndex: " + ImageIndex);
                    });
                }
            });
        }

        private void OnDownsizeSwitch()
        {
            Properties.Settings.Default.DownsizeImage = _downsizeImageToggle;

            if (ImagesFound.Count > 0)
                ImageSource = LoadImage(ActivePath);

            Properties.Settings.Default.Save();
        }

        private void OnZoomSensitivitySlider()
        {
            Properties.Settings.Default.ZoomSensitivity = _zoomSensitivity;

            Properties.Settings.Default.Save();
        }

        private void OnClipOpened(object sender, RoutedEventArgs e)
        {
            GetImageInformation(ActivePath);
        }

        /// <summary>
        /// Gets the gif image information (width, height, orientation)
        /// </summary>
        private void GetImageInformation(string path)
        {
            if (ImagesFound.Count == 0) return;

#if DEBUG
            Stopwatch stopwatch = new Stopwatch();//DEBUG
            stopwatch.Start();//DEBUG
#endif

            using (var imageStream = File.OpenRead(path))
            {
                //var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                //ImgWidth = decoder.Frames[0].PixelWidth;
                //ImgHeight = decoder.Frames[0].PixelHeight;

                Image img = Image.FromStream(imageStream);
                
                ImgWidth = img.Width;
                ImgHeight = img.Height;
                try
                {
                    ExifOrientations eo = ImageOrientation(img);
                    ImageRotation = OrientationDictionary[(int)eo];//eo angle from index

#if DEBUG
                    //notifier.ShowInformation($"Image Orientation: [angle: {ImageRotation}] {eo}");
#endif
                }
                catch
                {
                    string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.ImgOrientationFailedMsg));
                    notifier.ShowError(cultureTranslated);
                }
                img.Dispose();
            }

#if DEBUG
            stopwatch.Stop();//DEBUG
            notifier.ShowError($"GetImageInformation time: {stopwatch.ElapsedMilliseconds}ms");//DEBUG
#endif

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

        private void OnImageToClipboardCall()
        {
            //ToClipboard.CopyToClipboard(ActivePath);
            if (IsAnimated)
                ToClipboard.ImageToClipboard(new BitmapImage(MediaSource));
            else
                ToClipboard.ImageToClipboard(ImageSource);
        }

        private void OnCopyToClipboard(object sender, RoutedEventArgs e)
        {
            OnImageToClipboardCall();
        }

        private void OnCheckUpdateClick(object sender, RoutedEventArgs e)
        {
            AppUpdater.CheckForUpdates();
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            if (_shownLanguageDropIndex >= ShownLanguage.Count - 1)
                ShownLanguageDropIndex = 0;
            else
                ShownLanguageDropIndex++;

            ShownLanguageDrop.SelectedIndex = _shownLanguageDropIndex;

            //ChangeAccent();//called in OnAccentChanged
        }

        private void OnAccentClick(object sender, RoutedEventArgs e)
        {
            if (_themeAccentDropIndex >= ThemeAccents.Count - 1)
                ThemeAccentDropIndex = 0;
            else
                ThemeAccentDropIndex++;

            ThemeAccentDrop.SelectedIndex = _themeAccentDropIndex;

            //ChangeAccent();//called in OnAccentChanged
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || _shortcutButtonsOn == false) return;

            DeleteToRecycleAsync(ActivePath);
        }

        private void OnOpenFileLocation(object sender, RoutedEventArgs e)
        {
            ExploreFile();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_shortcutButtonsOn == false)
            {
                if (e.Key == Key.System || e.Key == Key.LWin || e.Key == Key.RWin) return;//blacklisted keys

                if(e.Key != Key.Escape)
                {
                    editingButton.Tag = e.Key;
                    //MessageBox.Show(((int)e.Key).ToString());
                }

                ShortcutButtonsOn = true;

                return;
            }

            if (IsDeletingFile) return;

            if (e.Key == _goForwardKey)
            {
                ChangeImage(1);//go forward
            }
            if (e.Key == _goBackwardKey)
            {
                ChangeImage(-1);//go back
            }

            if (e.Key == _pauseKey)
            {
                TogglePause();
            }

            if (e.Key == _deleteKey && ImagesFound.Count > 0)
            {
                DeleteToRecycleAsync(ActivePath);
            }

            if (e.Key == _stretchImageKey)
            {
                StretchImageToggle = !StretchImageToggle;
            }

            if (e.Key == _downsizeImageKey)
            {
                DownsizeImageToggle = !DownsizeImageToggle;
            }

            if (e.Key == _exploreFileKey)
            {
                ExploreFile();
            }

            if(e.Key == _copyToClipboardKey)
            {
                OnImageToClipboardCall();
            }
        }

        private void OnClick_Next(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || _shortcutButtonsOn == false) return;

            ChangeImage(1);//go forward
        }

        private void OnClick_Prev(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || _shortcutButtonsOn == false) return;

            ChangeImage(-1);//go back
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsDeletingFile || _shortcutButtonsOn == false) return;

            if (e.ChangedButton == MouseButton.XButton1)
            {
                ChangeImage(-1);//go back
            }
            if (e.ChangedButton == MouseButton.XButton2)
            {
                ChangeImage(1);//go forward
            }
        }

        private void OnOpenBrowseImage(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || _shortcutButtonsOn == false) return;

            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                OpenNewFile(openFileDialog.FileName);
            }
            /*else
            {
                //cancelled dialog
            }*/

            //GC.Collect();
        }

        private void OnShortcutClick(object sender, RoutedEventArgs e)
        {
            editingButton = (System.Windows.Controls.Button)sender;

            ShortcutButtonsOn = false;//disable the buttons until done editing

            //TODO: put text when editing for user to know; save changed buttons; add reset button for key resets

            //Binding myBinding = BindingOperations.GetBinding(b, System.Windows.Controls.Button.ContentProperty);
            //string p = myBinding.Path.Path;
        }

        /*private int ParseStringToOnlyInt(string input)
        {
            return int.Parse(string.Join("", input.Where(x => char.IsDigit(x))));
        }*/

        // Orientations
        public const int OrientationId = 0x0112;// 274 / 0x0112
        public enum ExifOrientations
        {
            Unknown = 0,//0
            TopLeft = 1,//0
            TopRight = 2,//90
            BottomRight = 3,//180
            BottomLeft = 4,//270
            LeftTop = 5,//0
            RightTop = 6,//90
            RightBottom = 7,//180
            LeftBottom = 8,//270
        }

        readonly Dictionary<int, Rotation> OrientationDictionary = new Dictionary<int, Rotation>()
        {
            {0, Rotation.Rotate0},
            {1, Rotation.Rotate0},
            {2, Rotation.Rotate90},
            {3, Rotation.Rotate180},
            {4, Rotation.Rotate270},
            {5, Rotation.Rotate0},
            {6, Rotation.Rotate90},
            {7, Rotation.Rotate180},
            {8, Rotation.Rotate270}
        };

        // Return the image's orientation
        public static ExifOrientations ImageOrientation(Image img)
        {
            // Get the index of the orientation property
            int orientation_index = Array.IndexOf(img.PropertyIdList, OrientationId);

            // If there is no such property, return Unknown
            if (orientation_index < 0) return ExifOrientations.Unknown;

            // Return the orientation value
            return (ExifOrientations)
                img.GetPropertyItem(OrientationId).Value[0];
        }

        public class NameComparer : IComparer<string>
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
            static extern int StrCmpLogicalW(string x, string y);

            public int Compare(string x, string y)
            {
                return StrCmpLogicalW(x, y);
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

    public class TextToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (string)value;
            var c = new System.Windows.Media.Color
            {
                A = 255
            };
            if (color == "Emerald")
            {
                c.R = 7;
                c.G = 117;
                c.B = 7;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else if (color == "Cobalt")
            {
                c.R = 7;
                c.G = 71;
                c.B = 198;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else if (color == "Amber")
            {
                c.R = 199;
                c.G = 137;
                c.B = 15;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else if (color == "Steel")
            {
                c.R = 87;
                c.G = 101;
                c.B = 115;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else if (color == "Mauve")
            {
                c.R = 101;
                c.G = 84;
                c.B = 117;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else if (color == "Taupe")
            {
                c.R = 115;
                c.G = 104;
                c.B = 69;
                SolidColorBrush brush = new SolidColorBrush(c);

                return brush;
            }
            else
            {
                return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class KeyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Key)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
