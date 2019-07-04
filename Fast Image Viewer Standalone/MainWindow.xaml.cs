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

        public List<string> ThemeAccents
        {
            get
            {
                return new List<string> { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };
            }
            set
            {
                ThemeAccents = value;
            }
        }

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

        //private string StartupPath;//program startup path

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        private readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif"/*, ".tiff"*/, ".bmp"/*, ".svg"*/, ".ico"/*, ".mp4", ".avi" */, ".JPG", ".JPEG", ".GIF", ".BMP", ".ICO", ".PNG" };//TODO: doesnt work: tiff svg
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Images (*.JPG, *.JPEG, *.PNG, *.GIF, *.BMP, *ICO)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.ICO"/* + "|All files (*.*)|*.*" */};

        private bool IsDeletingFile { get; set; } = false;

        private string ActiveFile { get; set; } = "";
        private string ActiveFolder { get; set; } = "";
        private string ActivePath { get; set; } = "";//directory + file + extension

        private readonly FileSystemWatcher fsw = new FileSystemWatcher()
        {
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite
            , IncludeSubdirectories = false
        };

        readonly Notifier notifier = new Notifier(cfg =>
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
            if (IsDeletingFile) return;

#if DEBUG
            Stopwatch stopwatch = new Stopwatch();//DEBUG
            stopwatch.Start();//DEBUG
#endif

            ActiveFile = Path.GetFileName(path);
            ActiveFolder = Path.GetDirectoryName(path);
            ActivePath = path;

            fsw.Path = ActiveFolder;
            fsw.EnableRaisingEvents = true;//File Watcher is enabled/disabled

            GetDirectoryFiles(ActiveFolder);

            FindIndexInFiles(ActiveFile);
            SetTitleInformation();

            NewUri(path);

#if DEBUG
            stopwatch.Stop();//DEBUG
            notifier.ShowError($"OpenNewFile time: {stopwatch.ElapsedMilliseconds}ms");//DEBUG
#endif
        }

        private void Fsw_Updated(object sender, FileSystemEventArgs e)//TODO: create more sophisticated updating where it doesnt load the whole directory again
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GetDirectoryFiles(ActiveFolder);

                FindIndexInFiles(ActiveFile);
                SetTitleInformation();

                ChangeImage(0);

                //MessageBox.Show("Updated: " + e.ChangeType.ToString() + " " + e.Name);
                notifier.ShowInformation($"{e.ChangeType.ToString()} \"{e.Name}\"");
            });
        }

        private void GetDirectoryFiles(string searchFolder)
        {
            ImagesFound.Clear();
            List<string> filesFound = new List<string>();

            //filesFound.AddRange(Directory.GetFiles(searchFolder, "*.*", SearchOption.TopDirectoryOnly));
            //filesFound.AddRange(Directory.EnumerateFiles(searchFolder).OrderBy(filename => filename));
            //filesFound.OrderBy(p => p.Substring(0)).ToList();//probably doesnt work
            filesFound.AddRange(System.IO.Directory.EnumerateFiles(searchFolder));

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
            Title = $"[{ImageIndex + 1}/{ImagesFound.Count}] {ImagesFound[ImageIndex]}";
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

                OnClipOpened(null, null);

                //MediaView?.Close();
                MediaSource = null;
                ImageSource = LoadImage(uri);

                borderImg.Reset();
            }

            //GC.Collect();
        }

        private BitmapImage LoadImage(Uri uri)
        {
            BitmapImage imgTemp = new BitmapImage();
            imgTemp.BeginInit();
            imgTemp.CacheOption = BitmapCacheOption.OnLoad;//TODO: remove this so it loads faster - needs to make workaround for deleting file from file lockup
            //imgTemp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;//TODO: remove this so it loads faster - needs to make workaround for deleting file
            imgTemp.UriSource = uri;

            if (_downsizeImageToggle)
            {
                if(ImgWidth > borderImg.ActualWidth)
                    imgTemp.DecodePixelWidth = (int)borderImg.ActualWidth;
                else if(ImgHeight > borderImg.ActualHeight)
                    imgTemp.DecodePixelHeight = (int)borderImg.ActualHeight;
            }
            if(ImageRotation != Rotation.Rotate0)
                imgTemp.Rotation = ImageRotation;

            imgTemp.EndInit();
            imgTemp.Freeze();

            return imgTemp;
        }

#region Flyout Events
        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            HelpFlyout.IsOpen = false;
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
        }

        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = false;
            HelpFlyout.IsOpen = !HelpFlyout.IsOpen;
        }

        private void OnCloseFlyoutsClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SettingsFlyout.IsOpen = false;
            HelpFlyout.IsOpen = false;
        }

        private void OnDonateClick(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6ZXTCHB3JXL4Q&source=url");
            Process.Start(sInfo);
        }
#endregion

        private void LoadAllSettings()
        {
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
                        notifier.ShowWarning($"File not found: {path}");
                    }

                    IsDeletingFile = false;
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.Message + "\nIndex: " + ImageIndex);
                    notifier.ShowError(e.Message + "\nIndex: " + ImageIndex);
                }
            });
        }

        private void OnDownsizeSwitch()
        {
            Properties.Settings.Default.DownsizeImage = _downsizeImageToggle;

            if (ImagesFound.Count > 0)
                ImageSource = LoadImage(new Uri(ActivePath, UriKind.Absolute));

            Properties.Settings.Default.Save();
        }

        private void OnZoomSensitivitySlider()
        {
            Properties.Settings.Default.ZoomSensitivity = _zoomSensitivity;

            Properties.Settings.Default.Save();
        }

        #region In-Window Events
        /// <summary>
        /// Gets the gif image dimensions
        /// </summary>
        private void OnClipOpened(object sender, RoutedEventArgs e)
        {
            if (ImagesFound.Count == 0) return;

#if DEBUG
            Stopwatch stopwatch = new Stopwatch();//DEBUG
            stopwatch.Start();//DEBUG
#endif
            using (var imageStream = File.OpenRead(ActivePath))
            {
                /*var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                ImgWidth = decoder.Frames[0].PixelWidth;
                ImgHeight = decoder.Frames[0].PixelHeight;*/

                Image img = Image.FromStream(imageStream);
                ImgWidth = img.Width;
                ImgHeight = img.Height;
                try
                {
                    ExifOrientations eo = ImageOrientation(img);
                    ImageRotation = OrientationDictionary[(int)eo];//eo angle from index
                    //PictureView.RenderTransform.Value.Rotate(OrientationDictionary[eoi]);

#if DEBUG
                    notifier.ShowInformation($"Image Orientation: [angle: {ImageRotation}] {eo}");
#endif
                }
                catch
                {
                    notifier.ShowError($"Could not get image orientation");
                }
                img.Dispose();
            }
            
#if DEBUG
            stopwatch.Stop();//DEBUG
            notifier.ShowError($"OnClipOpened time: {stopwatch.ElapsedMilliseconds}ms");//DEBUG
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

        private void OnAccentClick(object sender, RoutedEventArgs e)
        {
            if (_themeAccentDropIndex >= ThemeAccents.Count - 1)
                ThemeAccentDropIndex = 0;
            else
                ThemeAccentDropIndex++;

            ThemeAccentDrop.SelectedIndex = ThemeAccentDropIndex;

            //ChangeAccent();//called in OnAccentChanged
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile) return;

            DeleteToRecycleAsync(ActivePath);
        }

        private void OnOpenFileLocation(object sender, RoutedEventArgs e)
        {
            ExploreFile();
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsDeletingFile) return;

            if (e.Key == System.Windows.Input.Key.Right)
            {
                ChangeImage(1);//go forward
            }
            if (e.Key == System.Windows.Input.Key.Left)
            {
                ChangeImage(-1);//go back
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                TogglePause();
            }

            if (e.Key == System.Windows.Input.Key.Delete && ImagesFound.Count > 0)
            {
                DeleteToRecycleAsync(ActivePath);
            }

            if (e.Key == System.Windows.Input.Key.F)
            {
                StretchImageToggle = !StretchImageToggle;
            }

            if (e.Key == System.Windows.Input.Key.E)
            {
                ExploreFile();
            }
        }

        private void OnClick_Next(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile) return;

            ChangeImage(1);//go forward
        }

        private void OnClick_Prev(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile) return;

            ChangeImage(-1);//go back
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsDeletingFile) return;

            if (e.ChangedButton == System.Windows.Input.MouseButton.XButton1)
            {
                ChangeImage(-1);//go back
            }
            if (e.ChangedButton == System.Windows.Input.MouseButton.XButton2)
            {
                ChangeImage(1);//go forward
            }
        }

        private void OnOpenBrowseImage(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile) return;

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
        #endregion

        /*private int ParseStringToOnlyInt(string input)
        {
            return int.Parse(string.Join("", input.Where(x => char.IsDigit(x))));
        }*/

        // Orientations.
        public const int OrientationId = 0x0112;
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

        // Return the image's orientation.
        public static ExifOrientations ImageOrientation(Image img)
        {
            // Get the index of the orientation property.
            int orientation_index =
                Array.IndexOf(img.PropertyIdList, OrientationId);

            // If there is no such property, return Unknown.
            if (orientation_index < 0) return ExifOrientations.Unknown;

            // Return the orientation value.
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
}
