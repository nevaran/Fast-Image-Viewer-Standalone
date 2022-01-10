using FIVStandard.Comparers;
using FIVStandard.Core;
using FIVStandard.Model;
using FIVStandard.Utils;
using FIVStandard.ViewModel;
using MahApps.Metro.Controls;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

#pragma warning disable CA1416

namespace FIVStandard
{
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        #region Images
        private ThumbnailItemData imageItem;

        /// <summary>
        /// The class variable is always the currently viewed image
        /// </summary>
        public ThumbnailItemData ImageItem
        {
            get => imageItem;
            set
            {
                if (imageItem == value) return;

                imageItem = value;
                OnPropertyChanged();
                OnPropertyChanged("TitleInformation");
            }
        }

        public ListCollectionView ImagesDataView { get; }//sorted list - use this

        public ObservableCollection<ThumbnailItemData> ImagesData { get; } = new();

        public string TitleInformation
        {
            get
            {
                if (isDeletingFile)
                {
                    return $"{Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.Deleting), Localization.TranslationSource.Instance.CurrentCulture)} {ActiveFile}...";
                }
                else
                {
                    if (ImageItem is null)//no image
                    {
                        return "FIV";
                    }
                    else
                    {
#pragma warning disable IDE0071 // Breaks the binding somehow last time it was simplified
                        return $"[{(ImagesDataView.CurrentPosition + 1).ToString()}/{ImagesDataView.Count.ToString()}] {activeFile}";
#pragma warning restore IDE0071 // Simplify interpolation
                    }
                }
            }
        }
        #endregion

        private BitmapSource imageSource;

        public BitmapSource ImageSource//used only for non-animated files
        {
            get => imageSource;
            set
            {
                imageSource = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan mediaTimeElapsed;

        public TimeSpan MediaTimeElapsed
        {
            get => mediaTimeElapsed;
            set
            {
                mediaTimeElapsed = value;
                OnPropertyChanged();
                OnPropertyChanged("MediaTimeElapsedFormat");
            }
        }

        private TimeSpan mediaTimeElapsedMax;//total time (as TimeSpan) of the current media

        public TimeSpan MediaTimeElapsedMax
        {
            get => mediaTimeElapsedMax;
            set
            {
                mediaTimeElapsedMax = value;
                OnPropertyChanged();
                OnPropertyChanged(MediaTimeElapsedFormat);
            }
        }

        public string MediaTimeElapsedFormat => $"{MediaTimeElapsed:hh\\:mm\\:ss} / {MediaTimeElapsedMax:hh\\:mm\\:ss}";

        public ImageInformation ImageInfo { get; set; }

        public UpdateCheck AppUpdater { get; set; }

        public SettingsManager Settings { get; set; }

        public bool ThumbnailSlider_DragStarted { get; private set; } = false;//used for the ThumbnailSlider option to avoid glitching out

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        private Button editingButton;//current button control being edited - used for editing shortcuts

        private readonly DebounceDispatcher sharedDebouncer = new();

        public bool programLoaded = false;//initial loading of the app is done with it's required modules loaded aswell

        private bool selectedNew = false;//used to avoid ListBox event to re-select the image, doubling the loading time

        private bool isLoading = false;

        public bool IsLoading//is the program loading an image/media - used for the loading spinner
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged();
            }
        }

        private bool isDeletingFile;

        public bool IsDeletingFile//used for locking certain controls while the application is deleting a file
        {
            get => isDeletingFile;
            set
            {
                if (isDeletingFile == value) return;

                isDeletingFile = value;
                OnPropertyChanged("TitleInformation");
            }
        }

        private string activeFile = "FIV";
        /// <summary>
        /// File name + extension
        /// </summary>
        public string ActiveFile
        {
            get => activeFile;
            set
            {
                if (activeFile == value) return;

                activeFile = value;
                OnPropertyChanged("TitleInformation");
            }
        }

        /// <summary>
        /// Directory path only
        /// </summary>
        private string ActiveFolder { get; set; } = "";

        /// <summary>
        /// Directory + file name + extension
        /// </summary>
        public string ActivePath { get; set; } = "";

        private int TabControlSelectedTab;//used for checking if we are in the General tab or other. Disables certain features if not in General

        public static string DonationLink { get => "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6ZXTCHB3JXL4Q&source=url"; }

        private readonly FileSystemWatcher fsw = new()
        {
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite,
            IncludeSubdirectories = false
        };

        private OpenFileDialog _openFileWindow;
        public OpenFileDialog OpenFileWindow
        {
            get
            {
                if (_openFileWindow == null)//initialize only if module is required to improve startup speed
                {
                    _openFileWindow = new OpenFileDialog() { Filter = "Images|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.TIFF;*.ICO;*.SVG;*.WEBP;*.WEBM"/* + "|All files (*.*)|*.*" */};
#if DEBUG
                    Debug.WriteLine("FILE DIALOG CREATED");
#endif
                }

                return _openFileWindow;
            }
            set => _openFileWindow = value;
        }

        private NotificationManager _notificationManager;
        public NotificationManager NotificationManager
        {
            get
            {
                if (_notificationManager == null)//initialize only if module is required to improve startup speed
                {
                    _notificationManager = new NotificationManager(Notifications.Wpf.Core.Controls.NotificationPosition.BottomRight);
#if DEBUG
                    Debug.WriteLine("NOTIF MANAGER CREATED");
#endif
                }

                return _notificationManager;
            }
            set => _notificationManager = value;
        }

        private NotificationContent _notificationContent;
        public NotificationContent NotificationContent
        {
            get
            {
                if (_notificationContent == null)//initialize only if module is required to improve startup speed
                {
                    _notificationContent = new NotificationContent();
                }

                return _notificationContent;
            }
            set => _notificationContent = value;
        }

        #region Unused Unfinished Mutex
        //private readonly Mutex fivMutex;

        /*fivMutex = new Mutex(true, "FastImageViewerCoreApplication", out bool aIsNewInstance);
            if (!aIsNewInstance)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    string[] args = Environment.GetCommandLineArgs();
                    if (args.Length > 1)
                    {
                        //fivClient.Send(args[1]);
                    }

                    App.Current.Shutdown();
                }
                //fivMutex.ReleaseMutex();
            }*/
        #endregion

        public MainWindow()
        {
            ImageInfo = new ImageInformation();

            Settings = new SettingsManager(this);
            SettingsStore.InitSettingsStore(Settings.JSettings);
            ThumbnailItemData.Settings = Settings.JSettings;

            ImagesDataView = CollectionViewSource.GetDefaultView(ImagesData) as ListCollectionView;
            try
            {
                ImagesDataView.CustomSort = new LogicalComparer();
            }
            catch//in case shlwapi.dll is missing, use a safer but more inaccurate comparer
            {
                ImagesDataView.CustomSort = new NaturalOrderComparer(false);
            }

            AppUpdater = new UpdateCheck(this);

            //create new watcher events for used directory
            //fsw.Changed += Fsw_Updated;
            fsw.Created += Fsw_Created;
            fsw.Deleted += Fsw_Deleted;
            fsw.Renamed += Fsw_Renamed;

            InitializeComponent();

            DataContext = this;

            programLoaded = true;

            //AppWindow = this;//used for debugging ZoomBorder
        }

        private async void OnAppLoaded(object sender, RoutedEventArgs e)
        {
            _ = Settings.JSettings.CheckForUpdatesStartToggle
                ? AppUpdater.CheckForUpdates(UpdateCheckType.ForcedVersionCheck)
                : AppUpdater.CheckForUpdates(UpdateCheckType.SilentVersionCheck);

#if DEBUG
            string path = @"D:\Google Drive\temp\alltypes\3.png";
            //path = @"D:\FrapsVids\sharex\Screenshots\2020-12-28_03-19-28.webm";

            await OpenNewFileAsync(path);
#endif

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                await OpenNewFileAsync(args[1]);
            }
        }

        public async Task OpenNewFileAsync(string path)
        {
            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            selectedNew = true;

            ActiveFile = Path.GetFileName(path);
            ActiveFolder = Path.GetDirectoryName(path);
            ActivePath = path;

            fsw.Path = ActiveFolder;
            fsw.EnableRaisingEvents = true;//File Watcher is enabled/disabled

            if (Settings.JSettings.FilterActiveArray.Length == 0)
            {
                await ClearAllMediaAsync();
                return;
            }

            GetDirectoryFiles(ActiveFolder);

            FindIndexInFiles(activeFile);

            await NewUriAsync(path, true);
        }

        /// <summary>
        /// Gets all files into a list and then creates another list containing custom class with only the files from the original list that are of valid type
        /// </summary>
        /// <param name="searchFolder">Target folder full path</param>
        private void GetDirectoryFiles(string searchFolder)
        {
            ImagesData.Clear();
            List<string> filesFound = new();

            filesFound.AddRange(Directory.EnumerateFiles(searchFolder));//add all files of the folder to a list

            int c = filesFound.Count;
            for (int i = 0; i < c; i++)//extract the files that are of valid type
            {
                string ext = Path.GetExtension(filesFound[i].ToLower());
                if (Settings.JSettings.FilterActiveArray.Any(ext.Contains))
                {
                    filesFound[i] = Path.GetFileName(filesFound[i]);//get just the file name + extension

                    ThumbnailItemData tt = new()
                    {
                        ThumbnailName = filesFound[i],
                        FileType = Tools.IsAnimatedExtension(ext),
                    };
                    ImagesData.Add(tt);
                }
            }
        }

        private void FindIndexInFiles(string openedFile)
        {
            int L = ImagesDataView.Count;
            for (int i = 0; i < L; i++)
            {
                string currentIndexedName = ((ThumbnailItemData)ImagesDataView.GetItemAt(i)).ThumbnailName;
                if (openedFile == currentIndexedName)
                {
                    ImageItem = (ThumbnailItemData)ImagesDataView.GetItemAt(i);//TODO: temporary hack fix since the binding to it doesnt load fast enough for the info check
                    _ = ImagesDataView.MoveCurrentToPosition(i);

                    ActiveFile = currentIndexedName;
                    ActivePath = Path.Combine(ActiveFolder, activeFile);

                    OnPropertyChanged("TitleInformation");//update title information

                    break;
                }
            }
        }

        /// <summary>
        /// Clear all data (as if program is opened without opening an image)
        /// </summary>
        private async Task ClearAllMediaAsync()
        {
            ImagesData.Clear();
            await CloseMediaAsync();
            ImageSource = null;
            ImageInfo.ImgWidth = 0;
            ImageInfo.ImgHeight = 0;
        }

        private async Task ClearViewerAsync()
        {
            await CloseMediaAsync();
            ImageSource = null;
            ImageInfo.ImgWidth = 0;
            ImageInfo.ImgHeight = 0;
        }

        private async Task OpenMediaAsync(Uri uri)
        {
            _ = await MediaView.Open(uri);

            IsLoading = false;
        }

        private async Task CloseMediaAsync()
        {
            if (!MediaView.IsClosing)
                _ = await MediaView.Close();
        }

        private void TogglePause()
        {
            if (ImageItem is null) return;

            if (ImageItem.FileType != FileMediaType.Image)
            {
                _ = MediaView.IsPaused ? (MediaView?.Play()) : (MediaView?.Pause());
            }
        }

        public async Task ChangeImageAsync(int jump, bool moveToIndex, bool resetZoom = true)
        {
            if (ImagesData.Count == 0)//no more images in the folder - go back to default null
            {
                await ClearAllMediaAsync();
                return;
            }

            int jumpIndex = jump;

            /*if (moveToIndex)//ghost function since its already handled by ListBox and collection
            {
                //ImagesDataView.MoveCurrentToPosition(jumpIndex);
            }*/
            if (!moveToIndex)
            {
                jumpIndex += ImagesDataView.CurrentPosition;

                //wrap around a limit between 0 and how many images there are (minus 1)
                if (jumpIndex < 0) jumpIndex = ImagesDataView.Count - 1;
                if (jumpIndex >= ImagesDataView.Count) jumpIndex = 0;

                ImageItem = (ThumbnailItemData)ImagesDataView.GetItemAt(jumpIndex);//TODO: temporary hack fix since the binding to it doesnt load fast enough for the info check
                _ = ImagesDataView.MoveCurrentToPosition(jumpIndex);
            }

            ActiveFile = ImageItem.ThumbnailName;
            ActivePath = Path.Combine(ActiveFolder, activeFile);

            await NewUriAsync(ActivePath, resetZoom);
        }

        private CancellationTokenSource loadImageTokenSource = new();

        private async Task NewUriAsync(string path, bool resetZoom)
        {
            try
            {
                if (!Tools.IsOfType(path, Settings.JSettings.FilterActiveArray))
                {
                    await ChangeImageAsync(0, false);
                    return;
                }

                IsLoading = true;

                //show different controls based on what type the file is
                Btn_Pause.Visibility = Tools.BoolToVisibility((int)ImageItem.FileType > 0);
                MediaTime.Visibility = Tools.BoolToVisibility((int)ImageItem.FileType > 0);
                MediaProgression.Visibility = Tools.BoolToVisibility((int)ImageItem.FileType > 0);
                VolumeProgression.Visibility = Tools.BoolToVisibility((int)ImageItem.FileType > 1);
                VolumeProgressionIcon.Visibility = Tools.BoolToVisibility((int)ImageItem.FileType > 1);

                borderImg.Visibility = Tools.BoolToVisibility(ImageItem.FileType == FileMediaType.Image);
                borderMed.Visibility = Tools.BoolToVisibility(ImageItem.FileType != FileMediaType.Image);

                if (ImageItem.FileType != FileMediaType.Image)
                {
                    Uri uri = new(path, UriKind.Absolute);

                    ImageSource = null;
                    await CloseMediaAsync();
                    await OpenMediaAsync(uri);

                    if (resetZoom)
                        borderMed.Reset();
                }
                else
                {
                    loadImageTokenSource?.Cancel();

                    loadImageTokenSource = new CancellationTokenSource();
                    CancellationToken ctLoadImage = loadImageTokenSource.Token;

                    if (ImagesData.Count > 0)
                    {
                        Tools.GetImageInformation(ActivePath, ImageItem);
                        ImageInfo.ImgWidth = ImageItem.ImageWidth;
                        ImageInfo.ImgHeight = ImageItem.ImageHeight;
                    }
                    if (ImageItem.FileType != FileMediaType.Image)//the image is animated, try to load it via FFME instead
                    {
                        await NewUriAsync(ActivePath, resetZoom);
                        return;
                    }

                    // check to see if the token has been cancelled
                    /*if (ctLoadImage.IsCancellationRequested) {
                        // handle early exit.
                    }*/

                    //Binding imageBinding = BindingOperations.GetBinding(PictureView, Image.SourceProperty);
                    /*BindingOperations.ClearBinding(PictureView, Image.SourceProperty);
                    Binding imageBinding = new Binding();
                    imageBinding.Source = this;
                    imageBinding.Path = new PropertyPath("ImageSource");
                    imageBinding.Mode = BindingMode.OneWay;
                    imageBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    imageBinding.IsAsync = true;
                    BindingOperations.SetBinding(PictureView, Image.SourceProperty, imageBinding);*/

                    await CloseMediaAsync();

                    // load the image
                    BitmapSource bitmapSource = await Task.Run(() => Tools.LoadImage(path, ImageInfo.ImgWidth, ImageInfo.ImgHeight, this, ctLoadImage));

                    // repeat the token check
                    if (!ctLoadImage.IsCancellationRequested)
                    {
                        // apply the image to the control's property
                        ImageSource = bitmapSource;

                        if (resetZoom)
                            borderImg.Reset();
                    }
                }

                selectedNew = false;

                ScrollToListView();

                void ScrollToListView()//scroll to the newly selected item
                {
                    int cp = ImagesDataView.CurrentPosition;

                    if (cp < 0)//dont do anything if not 0 or less aka nothing selected (-1)
                        return;

                    thumbnailList.ScrollIntoView(ImagesDataView.GetItemAt(cp));
                }

                //GC.Collect();

                //have a delayed garbage collection call to clean up BitmapSource that did not free up immediately after a new image has been loaded
                sharedDebouncer.Debounce(500, GcCollectNormal);
            }
            catch(Exception e)
            {
                selectedNew = false;
                IsLoading = false;

                NotificationContent.Title = e.Message;
                NotificationContent.Message = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.ErrorLoadingImageCorrupt), Localization.TranslationSource.Instance.CurrentCulture);
                NotificationContent.Type = NotificationType.Error;
                _ = NotificationManager.ShowAsync(NotificationContent);
            }
        }

        private void GcCollectNormal()
        {
            GC.Collect();
            //Debug.WriteLine($"MEM {GC.GetTotalMemory(false) / 1024 / 1024}mb");
        }

        private CancellationTokenSource allThumbnailTokenSource;

        public Task ReloadAllThumbnailsAsync()
        {
            allThumbnailTokenSource?.Cancel();

            allThumbnailTokenSource = new CancellationTokenSource();
            CancellationToken ctAllThumbnail = allThumbnailTokenSource.Token;

            return Task.Run(() =>
            {
                int c = ImagesDataView.Count;
                for (int i = 0; i < c; i++)
                {
                    ctAllThumbnail.ThrowIfCancellationRequested();

                    ThumbnailItemData tid = (ThumbnailItemData)ImagesDataView.GetItemAt(i);

                    Tools.LoadSingleThumbnailData(Path.Combine(ActiveFolder, tid.ThumbnailName), tid);
                }

            }, allThumbnailTokenSource.Token);
        }

        private async Task DeleteToRecycleAsync(string path)
        {
            if (!File.Exists(path)) return;

            if (MediaView.HasVideo)
            {
                IsDeletingFile = true;

                await CloseMediaAsync();
            }

            IsDeletingFile = true;

            if (FileSystem.FileExists(path))
            {
                FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
            }
            else
            {
                NotificationContent.Title = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.FileNotFoundMsg), Localization.TranslationSource.Instance.CurrentCulture);
                NotificationContent.Message = path;
                NotificationContent.Type = NotificationType.Warning;
                _ = NotificationManager.ShowAsync(NotificationContent);
            }

            IsDeletingFile = false;
        }

        private async Task ImageCopyToClipboardCallAsync()
        {
            if (ImageItem is null) return;

            string fileType = Path.GetExtension(ImageItem.ThumbnailName);

            if (!File.Exists(ActivePath)) return;

            if (ImageItem.FileType != FileMediaType.Image)//if its animated, grab the currently viewed frame instead
            {
                using System.Drawing.Bitmap bm = await MediaView.CaptureBitmapAsync();
                ToClipboard.ImageCopyToClipboard(Tools.BitmapToBitmapSource(bm));
            }
            else//copy the image as a png (TODO: keep it the same file type and compression to keep the original size)
            {
                ToClipboard.ImageCopyToClipboard(ImageSource);
            }

            NotificationContent.Title = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.CopiedToClipboard), Localization.TranslationSource.Instance.CurrentCulture);
            NotificationContent.Message = ActiveFile;
            NotificationContent.Type = NotificationType.Success;
            _ = NotificationManager.ShowAsync(NotificationContent);
        }

        /*private void FileCopyToClipboardCall()
        {
            ToClipboard.FileCopyToClipboard(ActivePath);
        }*/

        private async Task FileCutToClipboardCallAsync()
        {
            if (ImageItem is null || !File.Exists(ActivePath)) return;

            string fileType = Path.GetExtension(ImageItem.ThumbnailName);
            if (fileType is ".gif" or ".webm")//if its a media, free up the file before cutting it to the clipboard
            {
                await ClearViewerAsync();
            }

            ToClipboard.FileCutToClipBoard(ActivePath);

            NotificationContent.Title = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.CutToClipboard), Localization.TranslationSource.Instance.CurrentCulture);
            NotificationContent.Message = ActiveFile;
            NotificationContent.Type = NotificationType.Success;
            _ = NotificationManager.ShowAsync(NotificationContent);
        }

        #region XAML events
        private async void OnMedia_Drop(object sender, DragEventArgs e)
        {
            string draggedFileUrl = (string)e.Data.GetData(DataFormats.Html, false);
            //Debug.WriteLine(Tools.GetUrlSourceImage(draggedFileUrl));

            if (draggedFileUrl == null)//load normally
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Note that you can have more than one file.
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    // Assuming you have one file that you care about, pass it off; if there are no valid files it will still open the folder but have an empty list
                    if (files.Length >= 1)
                        await OpenNewFileAsync(files[0]);
                }
            }
            /*else//opened an image from a URL
            {
                //await OpenNewFile(Tools.GetUrlSourceImage(draggedFileUrl));
                //ImageInfo.FromUrl = true;
            }*/
        }

        private void OnMediaView_MediaOpened(object sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            if (ImagesData.Count > 0)
            {
                ImageInfo.ImgWidth = MediaView.NaturalVideoWidth;
                ImageInfo.ImgHeight = MediaView.NaturalVideoHeight;

                ImageItem.ImageWidth = ImageInfo.ImgWidth;
                ImageItem.ImageHeight = ImageInfo.ImgHeight;
            }

            MediaTimeElapsedMax = MediaView.NaturalDuration.Value;
            MediaProgression.Maximum = MediaTimeElapsedMax.TotalMilliseconds;
        }

        private void OnMediaProgression_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaTimeElapsed = TimeSpan.FromMilliseconds(MediaProgression.Value);//convert to seconds TODO: find more efficient way of updating
        }

        private async void OnCopyToClipboard(object sender, RoutedEventArgs e)
        {
            await ImageCopyToClipboardCallAsync();
        }

        private async void OnCutToClipboard(object sender, RoutedEventArgs e)
        {
            await FileCutToClipboardCallAsync();
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            if (Settings.JSettings.ShownLanguageDropIndex >= Settings.JSettings.ShownLanguage.Count - 1)
                Settings.JSettings.ShownLanguageDropIndex = 0;
            else
                Settings.JSettings.ShownLanguageDropIndex++;

            ShownLanguageDrop.SelectedIndex = Settings.JSettings.ShownLanguageDropIndex;
        }

        private void OnAccentClick(object sender, RoutedEventArgs e)
        {
            if (Settings.JSettings.ThemeAccentDropIndex >= Settings.JSettings.ThemeAccents.Length - 1)
                Settings.JSettings.ThemeAccentDropIndex = 0;
            else
                Settings.JSettings.ThemeAccentDropIndex++;

            ThemeAccentDrop.SelectedIndex = Settings.JSettings.ThemeAccentDropIndex;
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            await DeleteToRecycleAsync(ActivePath);
        }

        private void OnOpenFileLocation(object sender, RoutedEventArgs e)
        {
            Tools.ExploreFile(Path.GetFullPath(ActivePath));
        }

        private async void OnKeyDown(object sender, KeyEventArgs e)
        {
            /*if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                ProcessStartInfo pinfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(App.StartupPath, "Fast Image Viewer.exe"),
                    Arguments = "/SILENT /CLOSEAPPLICATIONS",//TODO add working /RESTARTAPPLICATIONS /LOG
                    Verb = "runas",
                    UseShellExecute = true,
                };
                Process.Start(pinfo);
            }*/

            if (Settings.JSettings.ShortcutButtonsOn == false)
            {
                if (e.Key is Key.System or Key.LWin or Key.RWin) return;//blacklisted keys (windows keys, system)

                if (e.Key != Key.Escape)
                {
                    editingButton.Tag = e.Key;
                    //MessageBox.Show(((int)e.Key).ToString());
                }

                Settings.JSettings.ShortcutButtonsOn = true;

                return;
            }

            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            if (TabControlSelectedTab == 0)
            {
                if (e.Key == Settings.JSettings.GoForwardKey)
                {
                    selectedNew = true;
                    await ChangeImageAsync(1, false);//go forward
                }
                if (e.Key == Settings.JSettings.GoBackwardKey)
                {
                    selectedNew = true;
                    await ChangeImageAsync(-1, false);//go back
                }

                if (e.Key == Settings.JSettings.PauseKey)
                {
                    TogglePause();
                }

                if (e.Key == Settings.JSettings.DeleteKey && ImagesData.Count > 0)
                {
                    await DeleteToRecycleAsync(ActivePath);
                }
            }

            if (e.Key == Settings.JSettings.StretchImageKey)
            {
                Settings.JSettings.StretchImageToggle = !Settings.JSettings.StretchImageToggle;
            }

            if (e.Key == Settings.JSettings.DownsizeImageKey)
            {
                Settings.JSettings.DownsizeImageToggle = !Settings.JSettings.DownsizeImageToggle;
            }

            if (e.Key == Settings.JSettings.ExploreFileKey)
            {
                Tools.ExploreFile(Path.GetFullPath(ActivePath));
            }

            if (e.Key == Settings.JSettings.CopyImageToClipboardKey)
            {
                await ImageCopyToClipboardCallAsync();
            }

            if (e.Key == Settings.JSettings.CutFileToClipboardKey)
            {
                await FileCutToClipboardCallAsync();
            }

            if (e.Key == Settings.JSettings.ThumbnailListKey)
            {
                Settings.JSettings.EnableThumbnailListToggle = !Settings.JSettings.EnableThumbnailListToggle;
            }
        }

        private async void OnClick_Prev(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            selectedNew = true;
            await ChangeImageAsync(-1, false);//go back
        }

        private async void OnClick_Next(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            selectedNew = true;
            await ChangeImageAsync(1, false);//go forward
        }

        private void OnClick_Pause(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            TogglePause();
        }

        private async void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            if (e.ChangedButton == MouseButton.XButton1)
            {
                selectedNew = true;
                await ChangeImageAsync(-1, false);//go back
            }
            if (e.ChangedButton == MouseButton.XButton2)
            {
                selectedNew = true;
                await ChangeImageAsync(1, false);//go forward
            }
        }

        private async void OnOpenBrowseImage(object sender, RoutedEventArgs e)
        {
            if (isDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            bool? result = OpenFileWindow.ShowDialog();
            if (result == true)
            {
                await OpenNewFileAsync(OpenFileWindow.FileName);
            }
            //else//cancelled dialog
        }

        private void OnShortcutClick(object sender, RoutedEventArgs e)
        {
            editingButton = (Button)sender;

            Settings.JSettings.ShortcutButtonsOn = false;//disable the buttons until done editing
        }

        private void OnRemoveShortcutClick(object sender, RoutedEventArgs e)
        {
            editingButton = (Button)sender;

            editingButton.Tag = Key.None;//set shortcut to none
        }

        private void OnResetSettingsClick(object sender, RoutedEventArgs e)
        {
            Settings.ResetToDefault();
        }

        private async void OnCheckUpdateClick(object sender, RoutedEventArgs e)
        {
            await AppUpdater.CheckForUpdates(UpdateCheckType.ForcedVersionCheck);
        }

        private async void OnForceDownloadSetupClick(object sender, RoutedEventArgs e)
        {
            await AppUpdater.CheckForUpdates(UpdateCheckType.FullUpdateForced);
        }

        private async void OnThumbnailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImagesDataView.CurrentPosition < 0) return;

            if (!selectedNew)//this should be called only when selecting new image from the thumbnail list
            {
                await ChangeImageAsync(0, true);
            }
        }

        //select event when using the mouse on the list box items
        private void OnListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem tid = (ListBoxItem)sender;
            ImageItem = (ThumbnailItemData)tid.Content;
        }

        private void OnThumbnailSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            ThumbnailSlider_DragStarted = true;
        }

        private void OnThumbnailSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ReloadAllThumbnailsAsync();
            ThumbnailSlider_DragStarted = false;
        }

        private void OnMediaVolumeIcon_Click(object sender, RoutedEventArgs e)
        {
            Settings.JSettings.MediaMuted = !Settings.JSettings.MediaMuted;
        }

        private void OnMainFIV_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                Settings.JSettings.WindowWidth = (int)Width;
                Settings.JSettings.WindowHeight = (int)Height;
            }
        }

        private void OnMainFIV_StateChanged(object sender, EventArgs e)
        {
            Settings.JSettings.WindowState = WindowState;
        }

        private async void OnMetroTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MetroTabControl tc = (MetroTabControl)sender;
            TabControlSelectedTab = tc.SelectedIndex;

            _ = TabControlSelectedTab == 0 ? (MediaView?.Play()) : (MediaView?.Pause());//resume playing a media, if there is any, when we get back to the general tab

            if (Settings.ReloadFolderFlag == false) return;//dont re-open folder with file if not flagged

            Settings.ReloadFolderFlag = false;
            if (File.Exists(ActivePath))
                await OpenNewFileAsync(ActivePath);
        }

        private void OnThumbnailItemVisible(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbi = sender as ListBoxItem;
            ThumbnailItemData tid = (ThumbnailItemData)lbi.Content;

            if (Path.GetExtension(tid.ThumbnailName) == ".webm")//load a shell thumbnail if we are dealing with video types
            {
                try
                {
                    ShellObject shellFile = ShellObject.FromParsingName(Path.Combine(ActiveFolder, tid.ThumbnailName));
                    tid.ThumbnailImage = shellFile.Thumbnail.BitmapSource;

                    shellFile.Dispose();
                }
                catch { }
            }
            else
                _ = Task.Run(() => Tools.LoadSingleThumbnailData(Path.Combine(ActiveFolder, tid.ThumbnailName), tid));
        }

        private void OnDonateClick(object sender, RoutedEventArgs e)
        {
            Tools.OpenHyperlink(DonationLink, NotificationContent, NotificationManager);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Tools.OpenHyperlink(e.Uri.OriginalString, NotificationContent, NotificationManager);
        }

        private void OnAssociateFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileAssociations.EnsureAssociationsSet(
                new FileAssociation
                {
                    Extension = (string)((Button)sender).Tag,
                    ProgId = "Fast Image Viewer",
                    FileTypeDescription = "Image viewer for efficient viewing",
                    ExecutableFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fast Image Viewer.exe")
                });

            bool assoc = FileAssociations.GetAssociation((string)((Button)sender).Tag);
            ((Button)sender).Visibility = assoc ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnAssociateFileButtonShown(object sender, RoutedEventArgs e)
        {
            bool assoc = FileAssociations.GetAssociation((string)((Button)sender).Tag);
            ((Button)sender).Visibility = assoc ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnAllTypeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            bool cb = (bool)((CheckBox)sender).IsChecked;

            Settings.JSettings.FilterJpg = cb;
            Settings.JSettings.FilterJpeg = cb;
            Settings.JSettings.FilterPng = cb;
            Settings.JSettings.FilterGif = cb;
            Settings.JSettings.FilterBmp = cb;
            Settings.JSettings.FilterTiff = cb;
            Settings.JSettings.FilterIco = cb;
            Settings.JSettings.FilterSvg = cb;
            Settings.JSettings.FilterWebp = cb;
            Settings.JSettings.FilterWebm = cb;
            Settings.JSettings.FilterAll = cb;
        }

        private void MediaImageView_MouseMove(object sender, MouseEventArgs e)
        {
            //MediaView.Cursor = null;
            PictureView.Cursor = null;
            sharedDebouncer.Debounce(3000, DefaultMouseLook);//call to hide the mouse after X ms

            void DefaultMouseLook()//hide the mouse
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //MediaView.Cursor = Cursors.None;
                    PictureView.Cursor = Cursors.None;
                });
            }
        }

        #region FileSystemWatcher
        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                if (!Tools.IsOfType(e.Name, Settings.JSettings.FilterActiveArray)) return;//ignore if the file is not a valid type

                ThumbnailItemData tid = new()
                {
                    ThumbnailName = e.Name,
                    FileType = Tools.IsAnimatedExtension(Path.GetExtension(e.Name).ToLower()),
                };
                ImagesData.Add(tid);

                _ = Task.Run(() => Tools.LoadSingleThumbnailData(e.FullPath, tid));

                if (ImageItem is null)
                {
                    selectedNew = true;
                    await ChangeImageAsync(0, false);
                }

                OnPropertyChanged("TitleInformation");//Force update title information

                NotificationContent.Title = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.CreatedWatcher), Localization.TranslationSource.Instance.CurrentCulture);
                NotificationContent.Message = e.Name;
                NotificationContent.Type = NotificationType.Information;
                _ = NotificationManager.ShowAsync(NotificationContent);
            });
        }

        private void Fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                bool dirty = false;

                for (int i = 0; i < ImagesData.Count; i++)
                {
                    if (e.Name == ImagesData[i].ThumbnailName)
                    {
                        ImagesData.RemoveAt(i);
                        dirty = true;

                        break;
                    }
                }

                if (!Tools.IsOfType(e.Name, Settings.JSettings.FilterActiveArray) && !dirty) return;//dont send a message if its not one of our files

                if (ImageItem is null || ImageItem.ThumbnailName == e.Name)//if the viewed item is the changed one, update it
                {
                    selectedNew = true;
                    await ChangeImageAsync(0, false);
                }

                OnPropertyChanged("TitleInformation");//force update title information

                NotificationContent.Title = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.DeletedWatcher), Localization.TranslationSource.Instance.CurrentCulture);
                NotificationContent.Message = e.Name;
                NotificationContent.Type = NotificationType.Information;
                _ = NotificationManager.ShowAsync(NotificationContent);
            });
        }

        private void Fsw_Renamed(object sender, RenamedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                try
                {
                    for (int i = 0; i < ImagesData.Count; i++)
                    {
                        if (e.OldName == ImagesData[i].ThumbnailName)
                        {
                            if (Tools.IsOfType(e.Name, Settings.JSettings.FilterActiveArray))
                            {
                                BitmapSource oldThumbnail = ImagesData[i].ThumbnailImage;//save the thumbnail so we dont have to generate it again

                                ThumbnailItemData tt = new()
                                {
                                    ThumbnailName = e.Name,
                                    FileType = ImagesData[i].FileType,

                                    ThumbnailImage = oldThumbnail//just replace with the old thumbnail to save performance
                                };

                                ImagesData[i] = tt;

                                if (ActiveFile == e.OldName)//if the viewed item is the changed one, update it
                                {
                                    ActiveFile = ImagesData[i].ThumbnailName;
                                }

                                if (ImageItem.ThumbnailName == e.Name)
                                {
                                    selectedNew = true;
                                    await ChangeImageAsync(0, false);
                                }

                                NotificationContent.Title = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.RenamedWatcher), Localization.TranslationSource.Instance.CurrentCulture);
                                NotificationContent.Message = e.Name;
                                NotificationContent.Type = NotificationType.Information;
                                _ = NotificationManager.ShowAsync(NotificationContent);
                            }
                            else//file has been renamed to something with a non-valid extension - remove it from the list
                            {
                                ImagesData.RemoveAt(i);
                                selectedNew = true;
                                await ChangeImageAsync(0, false);
                            }

                            break;
                        }
                    }
                }
                catch { }
            });
        }
        #endregion

        private void MainFIV_Closing(object sender, CancelEventArgs e)
        {
            //fivMutex?.Close();

            if (_notificationManager is not null)
                _ = NotificationManager.CloseAllAsync();

            //fsw.Created -= Fsw_Created;
            //fsw.Deleted -= Fsw_Deleted;
            //fsw.Renamed -= Fsw_Renamed;
            fsw?.Dispose();

            //ClearAllMedia();
            Settings.Save();
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