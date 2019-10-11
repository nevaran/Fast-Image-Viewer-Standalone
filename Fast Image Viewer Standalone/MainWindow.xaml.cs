using FIVStandard.Comparers;
using FIVStandard.Modules;
using FIVStandard.Views;
using Gu.Localization;
using MahApps.Metro.Controls;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace FIVStandard
{
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private int imageIndex = 0;

        public int ImageIndex
        {
            get
            {
                return imageIndex;
            }
            set
            {
                if (imageIndex == value) return;

                imageIndex = value;
                UpdateCurrentImage();
                OnPropertyChanged();
            }
        }

        public List<string> ImagesFound { get; set; } = new List<string>();

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

        private ObservableCollection<ThumbnailTemplate> thumbnailImages = new ObservableCollection<ThumbnailTemplate>();

        public ObservableCollection<ThumbnailTemplate> ThumbnailImages
        {
            get
            {
                return thumbnailImages;
            }
            set
            {
                thumbnailImages = value;
            }
        }

        public UpdateCheck AppUpdater { get; set; }

        public SettingsManager Settings { get; set; }

        public CopyFileToClipboard ToClipboard { get; set; }

        public string StartupPath;//program startup path

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        private readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif"/*, ".tiff"*/, ".bmp"/*, ".svg"*/, ".ico"/*, ".mp4", ".avi" */, ".JPG", ".JPEG", ".GIF", ".BMP", ".ICO", ".PNG" };//TODO: doesnt work: tiff svg
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Images (*.JPG, *.JPEG, *.PNG, *.GIF, *.BMP, *ICO)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.ICO"/* + "|All files (*.*)|*.*" */};

        private System.Windows.Controls.Button editingButton = null;

        private bool IsDeletingFile { get; set; } = false;

        private string ActiveFile { get; set; } = "";//file name + extension
        private string ActiveFolder { get; set; } = "";//directory
        public string ActivePath { get; set; } = "";//directory + file name + extension

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

            AppUpdater = new UpdateCheck(this);
            Settings = new SettingsManager(this);
            ToClipboard = new CopyFileToClipboard();

            //create new watcher events for used directory
            fsw.Changed += Fsw_Updated;
            fsw.Deleted += Fsw_Updated;
            fsw.Created += Fsw_Updated;
            fsw.Renamed += Fsw_Updated;

            DataContext = this;

            Settings.Load();

            //AppWindow = this;//used for debugging ZoomBorder
        }

        private void OnAppLoaded(object sender, RoutedEventArgs e)
        {
            if(Settings.CheckForUpdatesStartToggle)
                AppUpdater.CheckForUpdates(UpdateCheckType.SilentVersionCheck);

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 0)//get startup path
            {
                StartupPath = Path.GetDirectoryName(args[0]);

#if DEBUG
                string path = @"D:\Google Drive\temp\alltypes\3.png";

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

                ChangeImage(0, false);
            });
        }

        public void OpenNewFile(string path)
        {
            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

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

            ThumbnailImages.Clear();//remove all items in the ListBox

            c = ImagesFound.Count;
            for (int i = 0; i < c; i++)
            {
                ThumbnailTemplate tt = new ThumbnailTemplate
                {
                    ThumbnailName = ImagesFound[i],
                    ThumbnailImage = LoadThumbnail(Path.Combine(ActiveFolder, ImagesFound[i]))
                };
                ThumbnailImages.Add(tt);
            }
        }

        void UpdateCurrentImage()
        {
            ChangeImage(imageIndex, true);
        }

        private void FindIndexInFiles(string openedFile)
        {
            int L = ImagesFound.Count;
            for (int i = 0; i < L; i++)
            {
                if(openedFile == ImagesFound[i])
                {
                    ImageIndex = i;
                    ActiveFile = ImagesFound[imageIndex];
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

        private void ChangeImage(int jump, bool moveToIndex)
        {
            if (ImagesFound.Count == 0)//no more images in the folder - go back to default null
            {
                ClearAllMedia();
                return;
            }

            if (moveToIndex)
            {
                if (jump == -1) return;
                ImageIndex = jump;
            }
            else
            {
                ImageIndex += jump;

                if (imageIndex < 0) ImageIndex = ImagesFound.Count - 1;
                if (imageIndex >= ImagesFound.Count) ImageIndex = 0;
            }

            if (!FileSystem.FileExists(Path.Combine(ActiveFolder, ImagesFound[ImageIndex])))//keep moving onward until we find an existing file
            {
                //refresh the file lists in the directory
                //GetDirectoryFiles(Path.GetDirectoryName(imagesFound[imageIndex]));
                //FindIndexInFiles(imagesFound[imageIndex]);

                //remove nonexistent file from list - if there are more than 1
                if (ImagesFound.Count > 1)
                {
                    ImagesFound.RemoveAt(imageIndex);
                    SetTitleInformation();
                }

                ChangeImage(jump, false);

                return;
            }

            ActiveFile = ImagesFound[imageIndex];
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

        public BitmapImage LoadImage(string path)
        {
            BitmapImage imgTemp = new BitmapImage();
            FileStream stream = File.OpenRead(path);
            imgTemp.BeginInit();
            imgTemp.CacheOption = BitmapCacheOption.OnLoad;//TODO: remove this so it loads faster - needs to make workaround for deleting file from file lockup
            //imgTemp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;//TODO: remove this so it loads faster - needs to make workaround for deleting file
            imgTemp.StreamSource = stream;

            if (Settings.DownsizeImageToggle)
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

        public BitmapImage LoadThumbnail(string path)
        {
            BitmapImage imgTemp = new BitmapImage();
            FileStream stream = File.OpenRead(path);
            imgTemp.BeginInit();
            imgTemp.CacheOption = BitmapCacheOption.OnLoad;
            imgTemp.StreamSource = stream;

            imgTemp.DecodePixelWidth = 80;
            imgTemp.DecodePixelHeight = 80;

            using (var imageStream = File.OpenRead(path))
            {
                Image img = Image.FromStream(imageStream);

                //ImgWidth = img.Width;
                //ImgHeight = img.Height;
                try
                {
                    ExifOrientations eo = GetImageOreintation(img);
                    Rotation imgRotation = OrientationDictionary[(int)eo];//eo angle from index

                    if (imgRotation != Rotation.Rotate0)
                        imgTemp.Rotation = imgRotation;
                }
                catch
                {
                    string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.ImgOrientationFailedMsg));
                    notifier.ShowError(cultureTranslated);
                }
                img.Dispose();
            }

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
                        string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.Deleting));
                        Title = $"{cultureTranslated} {ActiveFile}...";
                    });

                    if (FileSystem.FileExists(path))
                    {
                        FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);

                        //remove deleted item from list
                        /*Application.Current.Dispatcher.Invoke(() => this is done in the file watcher now
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

        /// <summary>
        /// Gets the gif image information (width, height, orientation)
        /// </summary>
        private void GetImageInformation(string path)
        {
            if (ImagesFound.Count == 0) return;

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
                    ExifOrientations eo = GetImageOreintation(img);
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

        private void ImageCopyToClipboardCall()
        {
            //ToClipboard.CopyToClipboard(ActivePath);
            if (IsAnimated)
                ToClipboard.ImageCopyToClipboard(new BitmapImage(MediaSource));
            else
                ToClipboard.ImageCopyToClipboard(ImageSource);
        }

        /*private void FileCopyToClipboardCall()
        {
            ToClipboard.FileCopyToClipboard(ActivePath);
        }*/

        private void FileCutToClipboardCall()
        {
            ToClipboard.FileCutToClipBoard(ActivePath);
        }

        #region XAML events
        private void OnClipEnded(object sender, RoutedEventArgs e)
        {
            MediaView.Position = new TimeSpan(0, 0, 1);
            MediaView.Play();
        }

        private void OnDonateClick(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6ZXTCHB3JXL4Q&source=url");
            Process.Start(sInfo);
        }

        private void OnClipOpened(object sender, RoutedEventArgs e)
        {
            GetImageInformation(ActivePath);
        }

        private void OnCopyToClipboard(object sender, RoutedEventArgs e)
        {
            ImageCopyToClipboardCall();
        }

        private void OnCutToClipboard(object sender, RoutedEventArgs e)
        {
            FileCutToClipboardCall();
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            if (Settings.ShownLanguageDropIndex >= Settings.ShownLanguage.Count - 1)
                Settings.ShownLanguageDropIndex = 0;
            else
                Settings.ShownLanguageDropIndex++;

            ShownLanguageDrop.SelectedIndex = Settings.ShownLanguageDropIndex;

            //ChangeAccent();//called in OnAccentChanged
        }

        private void OnAccentClick(object sender, RoutedEventArgs e)
        {
            if (Settings.ThemeAccentDropIndex >= Settings.ThemeAccents.Count - 1)
                Settings.ThemeAccentDropIndex = 0;
            else
                Settings.ThemeAccentDropIndex++;

            ThemeAccentDrop.SelectedIndex = Settings.ThemeAccentDropIndex;

            //ChangeAccent();//called in OnAccentChanged
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

            DeleteToRecycleAsync(ActivePath);
        }

        private void OnOpenFileLocation(object sender, RoutedEventArgs e)
        {
            ExploreFile();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Settings.ShortcutButtonsOn == false)
            {
                if (e.Key == Key.System || e.Key == Key.LWin || e.Key == Key.RWin) return;//blacklisted keys

                if(e.Key != Key.Escape)
                {
                    editingButton.Tag = e.Key;
                    //MessageBox.Show(((int)e.Key).ToString());

                    Settings.UpdateAllKeysProperties();
                }

                Settings.ShortcutButtonsOn = true;

                return;
            }

            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

            if (e.Key == Settings.GoForwardKey)
            {
                ChangeImage(1, false);//go forward
            }
            if (e.Key == Settings.GoBackwardKey)
            {
                ChangeImage(-1, false);//go back
            }

            if (e.Key == Settings.PauseKey)
            {
                TogglePause();
            }

            if (e.Key == Settings.DeleteKey && ImagesFound.Count > 0)
            {
                DeleteToRecycleAsync(ActivePath);
            }

            if (e.Key == Settings.StretchImageKey)
            {
                Settings.StretchImageToggle = !Settings.StretchImageToggle;
            }

            if (e.Key == Settings.DownsizeImageKey)
            {
                Settings.DownsizeImageToggle = !Settings.DownsizeImageToggle;
            }

            if (e.Key == Settings.ExploreFileKey)
            {
                ExploreFile();
            }

            if(e.Key == Settings.CopyImageToClipboardKey)
            {
                ImageCopyToClipboardCall();
            }

            if(e.Key == Settings.CutFileToClipboardKey)
            {
                FileCutToClipboardCall();
            }
        }

        private void OnClick_Next(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

            ChangeImage(1, false);//go forward
        }

        private void OnClick_Prev(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

            ChangeImage(-1, false);//go back
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

            if (e.ChangedButton == MouseButton.XButton1)
            {
                ChangeImage(-1, false);//go back
            }
            if (e.ChangedButton == MouseButton.XButton2)
            {
                ChangeImage(1, false);//go forward
            }
        }

        private void OnOpenBrowseImage(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

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

            Settings.ShortcutButtonsOn = false;//disable the buttons until done editing

            //TODO: put text when editing for user to know; save changed buttons; add reset button for key resets

            //Binding myBinding = BindingOperations.GetBinding(b, System.Windows.Controls.Button.ContentProperty);
            //string p = myBinding.Path.Path;
        }

        private void OnRemoveShortcutClick(object sender, RoutedEventArgs e)
        {
            editingButton = (System.Windows.Controls.Button)sender;

            editingButton.Tag = Key.None;

            Settings.UpdateAllKeysProperties();
        }

        private void OnResetSettingsClick(object sender, RoutedEventArgs e)
        {
            Settings.ResetToDefault();
        }

        private void OnCheckUpdateClick(object sender, RoutedEventArgs e)
        {
            AppUpdater.CheckForUpdates(UpdateCheckType.SilentVersionCheck);
        }

        private void OnForceDownloadSetupClick(object sender, RoutedEventArgs e)
        {
            AppUpdater.CheckForUpdates(UpdateCheckType.FullUpdateForced);
        }
        #endregion

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
        public static ExifOrientations GetImageOreintation(Image img)
        {
            // Get the index of the orientation property
            int orientation_index = Array.IndexOf(img.PropertyIdList, OrientationId);

            // If there is no such property, return Unknown
            if (orientation_index < 0) return ExifOrientations.Unknown;

            // Return the orientation value
            return (ExifOrientations)
                img.GetPropertyItem(OrientationId).Value[0];
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
