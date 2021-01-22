using FIVStandard.Comparers;
using FIVStandard.Core;
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
        private ThumbnailItemData imageItem = null;

        /// <summary>
        /// The class is always the currently viewed image
        /// </summary>
        public ThumbnailItemData ImageItem
        {
            get
            {
                return imageItem;
            }
            set
            {
                if (imageItem == value) return;

                imageItem = value;
                OnPropertyChanged();
                OnPropertyChanged("TitleInformation");
            }
        }

        public ListCollectionView ImagesDataView { get; }//sorted list - use this

        public ObservableCollection<ThumbnailItemData> ImagesData { get; } = new ObservableCollection<ThumbnailItemData>();

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
                        return $"[{ImagesDataView.CurrentPosition + 1}/{ImagesDataView.Count}] {activeFile}";
                    }
                }
            }
        }

        #region Image Properties
        private BitmapSource imageSource = null;

        public BitmapSource ImageSource
        {
            get
            {
                return imageSource;
            }
            set
            {
                imageSource = value;
                OnPropertyChanged();
            }
        }

        private int imgWidth = 0;

        public int ImgWidth
        {
            get
            {
                return imgWidth;
            }
            set
            {
                imgWidth = value;
                OnPropertyChanged();
                OnPropertyChanged("ImgResStringFormat");
            }
        }

        private int imgHeight = 0;

        public int ImgHeight
        {
            get
            {
                return imgHeight;
            }
            set
            {
                imgHeight = value;
                OnPropertyChanged();
                OnPropertyChanged("ImgResStringFormat");
            }
        }

        public string ImgResStringFormat
        {
            get
            {
                if (ImgWidth == 0 || ImgHeight == 0)
                {
                    return Tools.RnJesus();
                }
                else
                    return $"{ImgWidth}x{ImgHeight}";
            }
        }
        #endregion

        public UpdateCheck AppUpdater { get; set; }

        public SettingsManager Settings { get; set; }

        private bool selectedNew = false;//used to avoid ListBox event to re-select the image, doubling the loading time
        private bool dragStarted = false;//used for the ThumbnailSlider option to avoid glitching out

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        private Button editingButton = null;//current button control being edited - used for editing shortcuts

        public bool ProgramLoaded = false;

        private bool isLoading = false;

        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            set
            {
                isLoading = value;
                OnPropertyChanged();
            }
        }

        private bool isDeletingFile;

        public bool IsDeletingFile
        {
            get
            {
                return isDeletingFile;
            }
            set
            {
                if (isDeletingFile == value) return;

                isDeletingFile = value;
                OnPropertyChanged("TitleInformation");
            }
        }

        private string activeFile = "FIV";
        /// <summary>
        /// file name + extension
        /// </summary>
        public string ActiveFile
        {
            get
            {
                return activeFile;
            }
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
        /// directory + file name + extension
        /// </summary>
        public string ActivePath { get; set; } = "";

        int TabControlSelectedTab = 0;

        public static string DonationLink { get => "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6ZXTCHB3JXL4Q&source=url"; }

        private readonly FileSystemWatcher fsw = new FileSystemWatcher()
        {
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite
            , IncludeSubdirectories = false
        };

        private OpenFileDialog _openFileWindow;
        public OpenFileDialog OpenFileWindow
        {
            get
            {
                if (_openFileWindow == null)
                {
                    _openFileWindow = new OpenFileDialog() { Filter = "Images|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.TIFF;*.ICO;*.SVG;*.WEBP;*.WEBM"/* + "|All files (*.*)|*.*" */};
#if DEBUG
                    Debug.WriteLine("FILE DIALOG CREATED");
#endif
                }

                return _openFileWindow;
            }
            set
            {
                _openFileWindow = value;
            }
        }

        private NotificationManager _notificationManager;
        public NotificationManager NotificationManager
        {
            get
            {
                if(_notificationManager == null)
                {
                    _notificationManager = new NotificationManager(Notifications.Wpf.Core.Controls.NotificationPosition.BottomRight);
#if DEBUG
                    Debug.WriteLine("NOTIF MANAGER CREATED");
#endif
                }

                return _notificationManager;
            }
            set
            {
                _notificationManager = value;
            }
        }

        private NotificationContent _notificationContent;
        public NotificationContent NotificationContent
        {
            get
            {
                if(_notificationContent == null)
                {
                    _notificationContent = new NotificationContent();
#if DEBUG
                    Debug.WriteLine("NOTIF CONTENT CREATED");
#endif
                }

                return _notificationContent;
            }
            set
            {
                _notificationContent = value;
            }
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

            ProgramLoaded = true;

            //AppWindow = this;//used for debugging ZoomBorder
        }

        private async void OnAppLoaded(object sender, RoutedEventArgs e)
        {
            if (Settings.JSettings.CheckForUpdatesStartToggle)
                _ = AppUpdater.CheckForUpdates(UpdateCheckType.ForcedVersionCheck);
            else
                _ = AppUpdater.CheckForUpdates(UpdateCheckType.SilentVersionCheck);

#if DEBUG
            string path = @"D:\Google Drive\temp\alltypes\3.png";
            //path = @"D:\FrapsVids\sharex\Screenshots\2020-12-28_03-19-28.webm";

            await OpenNewFile(path);
#endif

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                await OpenNewFile(args[1]);
            }
        }

        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                if (!Tools.IsOfType(e.Name, Settings.JSettings.FilterActiveArray)) return;//ignore if the file is not a valid type

                ThumbnailItemData tt = new ThumbnailItemData
                {
                    ThumbnailName = e.Name,
                    IsAnimated = Tools.IsAnimatedExtension(Path.GetExtension(e.Name).ToLower()),
                };
                ImagesData.Add(tt);

                _ = Task.Run(() => Tools.LoadSingleThumbnailData(e.FullPath, tt));

                if(ImageItem is null)
                {
                    selectedNew = true;
                    await ChangeImage(0, false);
                }

                OnPropertyChanged("TitleInformation");//update title information

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

                if(ImageItem is null || ImageItem.ThumbnailName == e.Name)
                {
                    selectedNew = true;
                    await ChangeImage(0, false);
                }

                OnPropertyChanged("TitleInformation");//update title information

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
                                var oldThumbnail = ImagesData[i].ThumbnailImage;//save the thumbnail so we dont have to generate it again

                                ThumbnailItemData tt = new ThumbnailItemData
                                {
                                    ThumbnailName = e.Name,
                                    IsAnimated = ImagesData[i].IsAnimated,

                                    ThumbnailImage = oldThumbnail//just replace with the old thumbnail to save performance
                                };

                                ImagesData[i] = tt;

                                //if the viewed item is the changed one, update it
                                if (ActiveFile == e.OldName)
                                {
                                    ActiveFile = ImagesData[i].ThumbnailName;
                                }

                                if (ImageItem.ThumbnailName == e.Name)
                                {
                                    selectedNew = true;
                                    await ChangeImage(0, false);
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
                                await ChangeImage(0, false);
                            }

                            break;
                        }
                    }
                }
                catch { }
            });
        }

        public async Task OpenNewFile(string path)
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
                await ClearAllMedia();
                return;
            }

            GetDirectoryFiles(ActiveFolder);

            FindIndexInFiles(activeFile);

            await NewUri(path, true);
        }

        /// <summary>
        /// Gets all files into a list and then creates another list containing custom class with only the files from the original list that are of valid type
        /// </summary>
        /// <param name="searchFolder">Target folder full path</param>
        private void GetDirectoryFiles(string searchFolder)
        {
            ImagesData.Clear();
            List<string> filesFound = new List<string>();

            filesFound.AddRange(Directory.EnumerateFiles(searchFolder));//add all files of the folder to a list

            int c = filesFound.Count;
            for (int i = 0; i < c; i++)//extract the files that are of valid type
            {
                string ext = Path.GetExtension(filesFound[i].ToLower());
                if (Settings.JSettings.FilterActiveArray.Any(ext.Contains))
                {
                    filesFound[i] = Path.GetFileName(filesFound[i]);//get just the file name + extension

                    ThumbnailItemData tt = new ThumbnailItemData
                    {
                        ThumbnailName = filesFound[i],
                        IsAnimated = Tools.IsAnimatedExtension(ext),
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
                    ImagesDataView.MoveCurrentToPosition(i);

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
        private async Task ClearAllMedia()
        {
            ImagesData.Clear();
            await CloseMedia();
            ImageSource = null;
            ImgWidth = 0;
            ImgHeight = 0;
        }

        private async Task ClearViewer()
        {
            await CloseMedia();
            ImageSource = null;
            ImgWidth = 0;
            ImgHeight = 0;
        }

        private async Task OpenMedia(Uri uri)
        {
            await MediaView.Open(uri);

            IsLoading = false;
        }

        private async Task CloseMedia()
        {
            if(!MediaView.IsClosing)
                await MediaView.Close();
        }

        private void TogglePause()
        {
            if (ImageItem is null) return;

            if (ImageItem.IsAnimated)
            {
                if (MediaView.IsPaused)
                {
                    MediaView?.Play();
                }
                else
                {
                    MediaView?.Pause();
                }
            }
        }

        public async Task ChangeImage(int jump, bool moveToIndex, bool resetZoom = true)
        {
            if (ImagesData.Count == 0)//no more images in the folder - go back to default null
            {
                await ClearAllMedia();
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
                ImagesDataView.MoveCurrentToPosition(jumpIndex);
            }

            ActiveFile = ImageItem.ThumbnailName;
            ActivePath = Path.Combine(ActiveFolder, activeFile);

            await NewUri(ActivePath, resetZoom);
        }

        CancellationTokenSource loadImageTokenSource = new CancellationTokenSource();

        private async Task NewUri(string path, bool resetZoom)
        {
            if (!Tools.IsOfType(path, Settings.JSettings.FilterActiveArray))
            {
                await ChangeImage(0, false);
                return;
            }

            IsLoading = true;

            if (ImageItem.IsAnimated)
            {
                MediaProgression.Visibility = Visibility.Visible;
                VolumeProgression.Visibility = Visibility.Visible;
                VolumeProgressionIcon.Visibility = Visibility.Visible;
            }
            else
            {
                MediaProgression.Visibility = Visibility.Hidden;
                VolumeProgression.Visibility = Visibility.Hidden;
                VolumeProgressionIcon.Visibility = Visibility.Hidden;
            }

            if (ImageItem.IsAnimated)
            {
                borderImg.Visibility = Visibility.Hidden;
                borderMed.Visibility = Visibility.Visible;

                Uri uri = new Uri(path, UriKind.Absolute);

                ImageSource = null;
                await CloseMedia();
                await OpenMedia(uri);

                if (resetZoom)
                    borderMed.Reset();
            }
            else
            {
                if (loadImageTokenSource is not null)
                {
                    loadImageTokenSource.Cancel();
                    loadImageTokenSource.Dispose();
                }

                loadImageTokenSource = new CancellationTokenSource();
                var ctLoadImage = loadImageTokenSource.Token;

                borderImg.Visibility = Visibility.Visible;
                borderMed.Visibility = Visibility.Hidden;

                if (ImagesData.Count > 0)
                {
                    Tools.GetImageInformation(ActivePath, ImageItem);
                    ImgWidth = ImageItem.ImageWidth;
                    ImgHeight = ImageItem.ImageHeight;
                }
                if (ImageItem.IsAnimated)//the image is animated, try to load it via ffm instead
                {
                    await NewUri(ActivePath, resetZoom);
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

                await CloseMedia();

                // load the image
                //ImageSource = null;
                BitmapSource bitmapSource = await Task.Run(() => Tools.LoadImage(path, ImgWidth, ImgHeight, this, ctLoadImage));

                // repeat the token check
                if (!ctLoadImage.IsCancellationRequested)
                {
                    // apply the image to the control's property
                    ImageSource = bitmapSource;

                    if(resetZoom)
                        borderImg.Reset();
                }
            }

            selectedNew = false;

            ScrollToListView();
            GC.Collect();
            //ddGcCollect.Debounce(100, GcCollectNormal);
        }

        //private readonly DebounceDispatcher ddGcCollect = new DebounceDispatcher();

        //private void GcCollectNormal()
        //{
        //    GC.Collect();
        //    Debug.WriteLine($"MEM {GC.GetTotalMemory(false) / 1024 / 1024}mb");
        //}

        CancellationTokenSource allThumbnailTokenSource;
        CancellationToken ct;

        public Task ReloadAllThumbnailsAsync()
        {
            allThumbnailTokenSource?.Cancel();

            allThumbnailTokenSource = new CancellationTokenSource();
            ct = allThumbnailTokenSource.Token;

            return Task.Run(() =>
            {
                int c = ImagesDataView.Count;
                for (int i = 0; i < c; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    ThumbnailItemData tid = ((ThumbnailItemData)ImagesDataView.GetItemAt(i));

                    Tools.LoadSingleThumbnailData(Path.Combine(ActiveFolder, tid.ThumbnailName), tid);
                }

            }, allThumbnailTokenSource.Token);
        }

        public void ExploreFile()
        {
            if (File.Exists(ActivePath))
            {
                //Clean up file path so it can be navigated OK
                Process.Start("explorer.exe", string.Format("/select,\"{0}\"", Path.GetFullPath(ActivePath)));
            }
        }

        private async Task DeleteToRecycleAsync(string path)
        {
            if (!File.Exists(path)) return;

            if(MediaView.HasVideo)
            {
                IsDeletingFile = true;

                await CloseMedia();
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

        private async void ImageCopyToClipboardCall()
        {
            if (ImageItem is null) return;

            string fileType = Path.GetExtension(ImageItem.ThumbnailName);

            if (!File.Exists(ActivePath)) return;

            if (ImageItem.IsAnimated)
            {
                using System.Drawing.Bitmap bm = await MediaView.CaptureBitmapAsync();
                ToClipboard.ImageCopyToClipboard(Tools.BitmapToBitmapSource(bm));
            }
            else
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

        private async Task FileCutToClipboardCall()
        {
            if (ImageItem is null || !File.Exists(ActivePath)) return;

            string fileType = Path.GetExtension(ImageItem.ThumbnailName);
            if (fileType == ".gif"  || fileType == ".webm")//TODO: temp fix
            {
                await ClearViewer();
            }

            ToClipboard.FileCutToClipBoard(ActivePath);

            NotificationContent.Title = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.CutToClipboard), Localization.TranslationSource.Instance.CurrentCulture);
            NotificationContent.Message = ActiveFile;
            NotificationContent.Type = NotificationType.Success;
            _ = NotificationManager.ShowAsync(NotificationContent);
        }

        private void OpenHyperlink(string url)
        {
            try
            {
                ProcessStartInfo sInfo = new ProcessStartInfo(url) { UseShellExecute = true };
                Process.Start(sInfo);
            }
            catch (Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                {
                    NotificationContent.Title = "Hyperlink Error";
                    NotificationContent.Message = noBrowser.Message;
                    NotificationContent.Type = NotificationType.Error;
                    NotificationManager.ShowAsync(NotificationContent);
                }
            }
            catch (Exception other)
            {
                NotificationContent.Title = "Hyperlink Error";
                NotificationContent.Message = other.Message;
                NotificationContent.Type = NotificationType.Error;
                NotificationManager.ShowAsync(NotificationContent);
            }
        }

        #region XAML events
        private async void Media_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off; if there are no valid files it will still open the folder but have an empty list
                if (files.Length >= 1)
                    await OpenNewFile(files[0]);
            }
        }

        private void OnDonateClick(object sender, RoutedEventArgs e)
        {
            OpenHyperlink(DonationLink);
        }

        private void MediaView_MediaOpened(object sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            if (ImagesData.Count > 0)
            {
                ImgWidth = MediaView.NaturalVideoWidth;
                ImgHeight = MediaView.NaturalVideoHeight;

                ImageItem.ImageWidth = ImgWidth;
                ImageItem.ImageHeight = ImgHeight;
            }
            MediaProgression.Maximum = MediaView.NaturalDuration.Value.Ticks;
        }

        private void OnCopyToClipboard(object sender, RoutedEventArgs e)
        {
            ImageCopyToClipboardCall();
        }

        private async void OnCutToClipboard(object sender, RoutedEventArgs e)
        {
            await FileCutToClipboardCall();
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
            ExploreFile();
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
                if (e.Key == Key.System || e.Key == Key.LWin || e.Key == Key.RWin) return;//blacklisted keys (windows keys, system)

                if(e.Key != Key.Escape)
                {
                    editingButton.Tag = e.Key;
                    //MessageBox.Show(((int)e.Key).ToString());
                }

                Settings.JSettings.ShortcutButtonsOn = true;

                return;
            }

            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            if(TabControlSelectedTab == 0)
            {
                if (e.Key == Settings.JSettings.GoForwardKey)
                {
                    selectedNew = true;
                    await ChangeImage(1, false);//go forward
                }
                if (e.Key == Settings.JSettings.GoBackwardKey)
                {
                    selectedNew = true;
                    await ChangeImage(-1, false);//go back
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
                ExploreFile();
            }

            if(e.Key == Settings.JSettings.CopyImageToClipboardKey)
            {
                ImageCopyToClipboardCall();
            }

            if(e.Key == Settings.JSettings.CutFileToClipboardKey)
            {
                await FileCutToClipboardCall();
            }

            if(e.Key == Settings.JSettings.ThumbnailListKey)
            {
                Settings.JSettings.EnableThumbnailListToggle = !Settings.JSettings.EnableThumbnailListToggle;
            }
        }

        private async void OnClick_Prev(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            selectedNew = true;
            await ChangeImage(-1, false);//go back
        }

        private async void OnClick_Next(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            selectedNew = true;
            await ChangeImage(1, false);//go forward
        }

        private async void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            if (e.ChangedButton == MouseButton.XButton1)
            {
                selectedNew = true;
                await ChangeImage(-1, false);//go back
            }
            if (e.ChangedButton == MouseButton.XButton2)
            {
                selectedNew = true;
                await ChangeImage(1, false);//go forward
            }
        }

        private async void OnOpenBrowseImage(object sender, RoutedEventArgs e)
        {
            if (isDeletingFile || Settings.JSettings.ShortcutButtonsOn == false) return;

            Nullable<bool> result = OpenFileWindow.ShowDialog();
            if (result == true)
            {
                await OpenNewFile(OpenFileWindow.FileName);
            }
            /*else
            {
                //cancelled dialog
            }*/
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

        private async void ThumbnailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImagesDataView.CurrentPosition < 0) return;

            if (!selectedNew)//this should be called only when selecting new image from the thumbnail list
            {
                await ChangeImage(0, true);
            }
        }

        private void ScrollToListView()
        {
            int cp = ImagesDataView.CurrentPosition;

            if (cp < 0)//dont do anything if not 0 or less aka nothing selected (-1)
                return;

            thumbnailList.ScrollIntoView(ImagesDataView.GetItemAt(cp));
        }

        //select event when using the mouse on the list box items
        private void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var tid = (ListBoxItem)sender;
            ImageItem = (ThumbnailItemData)tid.Content;
        }

        private void ThumbnailSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ReloadAllThumbnailsAsync();
            dragStarted = false;
        }

        private void ThumbnailSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            dragStarted = true;
        }

        public void ThumbnailResSlider_ValueChanged()
        {
            if (!dragStarted)
                ReloadAllThumbnailsAsync();
        }

        private void MediaVolumeIcon_Click(object sender, RoutedEventArgs e)
        {
            Settings.JSettings.MediaMuted = !Settings.JSettings.MediaMuted;
        }

        private void MainFIV_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(WindowState == WindowState.Normal)
            {
                Settings.JSettings.WindowWidth = (int)Width;
                Settings.JSettings.WindowHeight = (int)Height;
            }
        }

        private void MainFIV_StateChanged(object sender, EventArgs e)
        {
            Settings.JSettings.WindowState = WindowState;
        }

        private async void MetroTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MetroTabControl tc = (MetroTabControl)sender;
            TabControlSelectedTab = tc.SelectedIndex;
            if (TabControlSelectedTab == 0)
            {
                MediaView?.Play();
            }
            else
            {
                MediaView?.Pause();
            }

            if (Settings.ReloadFolderFlag == false) return;//dont re-open folder with file if not flagged

            Settings.ReloadFolderFlag = false;
            await OpenNewFile(ActivePath);
        }

        private void OnThumbnailItemVisible(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbi = sender as ListBoxItem;
            ThumbnailItemData dataItem = (ThumbnailItemData)lbi.Content;

            if (Path.GetExtension(dataItem.ThumbnailName) == ".webm")//load a shell thumbnail if we are dealing with video types
            {
                try
                {
                    var shellFile = ShellObject.FromParsingName(Path.Combine(ActiveFolder, dataItem.ThumbnailName));
                    dataItem.ThumbnailImage = shellFile.Thumbnail.BitmapSource;

                    shellFile.Dispose();
                }
                catch { }
            }
            else
                Task.Run(() => Tools.LoadSingleThumbnailData(Path.Combine(ActiveFolder, dataItem.ThumbnailName), dataItem));
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            OpenHyperlink(e.Uri.OriginalString);
        }

        private void OnAssociateFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileAssociations.EnsureAssociationsSet(
                new FileAssociation
                {
                    Extension = (string)(((Button)sender).Tag),
                    ProgId = "Fast Image Viewer",
                    FileTypeDescription = "Image viewer for efficient viewing",
                    ExecutableFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fast Image Viewer.exe")
                });

            bool assoc = FileAssociations.GetAssociation((string)(((Button)sender).Tag));
            ((Button)sender).Visibility = assoc ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnAssociateFileButtonShown(object sender, RoutedEventArgs e)
        {
            bool assoc = FileAssociations.GetAssociation((string)(((Button)sender).Tag));
            ((Button)sender).Visibility = assoc ? Visibility.Visible : Visibility.Collapsed;
        }

        private readonly DebounceDispatcher ddMouseMove = new DebounceDispatcher();

        private void MediaImageView_MouseMove(object sender, MouseEventArgs e)
        {
            MediaView.Cursor = null;//TODO: make cursor change in ffme control work
            PictureView.Cursor = null;
            ddMouseMove.Debounce(3000, DefaultMouseLook);
        }

        private void DefaultMouseLook()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MediaView.Cursor = Cursors.None;
                PictureView.Cursor = Cursors.None;
            });
        }

        private void MainFIV_Closing(object sender, CancelEventArgs e)
        {
            //fivMutex?.Close();

            if (_notificationManager is not null)
                NotificationManager.CloseAllAsync();

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