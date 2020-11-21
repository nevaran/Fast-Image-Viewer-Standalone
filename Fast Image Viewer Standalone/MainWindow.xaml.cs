using FIVStandard.Comparers;
using FIVStandard.Core;
using FIVStandard.Views;
using Gu.Localization;
using MahApps.Metro.Controls;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;
using Unosquare.FFME;

namespace FIVStandard
{
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private ThumbnailItemData imageItem;

        /// <summary>
        /// The class reference to the currently viewed image
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

                //ScrollToListView();
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
                    return $"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.Deleting))} {ActiveFile}...";
                }
                else
                {
                    if(ImageItem is null)//no image
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

        private readonly string[] rnj = new string[] {
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "owo",
                        "uwu",
                        "ゴ ゴ ゴ ゴ",
                        "( ͡° ͜ʖ ͡°)",
                        "I am Lagnar",
                        "Made by ねヴぁらん",
        };

        public string ImgResStringFormat
        {
            get
            {
                if (ImgWidth == 0 || ImgHeight == 0)
                {
                    return rnj[new Random().Next(0, rnj.Length)];//TODO: bring back the owo's
                }
                else
                    return $"{ImgWidth}x{ImgHeight}";
            }
        }
        #endregion

        private double totalMediaTime = 0;

        public UpdateCheck AppUpdater { get; set; }

        public SettingsManager Settings { get; set; }

        public CopyFileToClipboard ToClipboard { get; set; }

        public string StartupPath;//program startup path

        private bool selectedNew = false;//used to avoid ListBox event to re-select the image, doubling the loading time
        private bool dragStarted = true;//used for the ThumbnailSlider option to avoid glitching out

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        //private readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico", ".webp"/*, ".tiff", ".svg", ".mp4", ".avi" */ };//TODO: doesnt work: tiff svg
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Images|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.TIFF;*.ICO;*.SVG;*.WEBP;*.WEBM"/* + "|All files (*.*)|*.*" */};

        private Button editingButton = null;//used for editing shortcuts

        private bool isDeletingFile;
        private bool forDeletiionMediaFlag = false;
        private string forDeletionMediaPath = "";

        public bool ProgramLoaded = false;

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

        public string ActiveFile//file name + extension
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

        private string ActiveFolder { get; set; } = "";//directory
        public string ActivePath { get; set; } = "";//directory + file name + extension

        public string DonationLink
        {
            get
            {
                return "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6ZXTCHB3JXL4Q&source=url";
            }
        }

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
            //SingleInstance();//TODO: find way to send argument between programs

            InitializeComponent();

            RenderOptions.SetBitmapScalingMode(PictureView, BitmapScalingMode.Fant);

            StartupPath = System.AppDomain.CurrentDomain.BaseDirectory;

            ImageMagick.MagickAnyCPU.CacheDirectory = StartupPath;

            //Library.FFmpegLoadModeFlags = FFmpegLoadMode.MinimumFeatures;
            Library.FFmpegDirectory = @$"{StartupPath}\ffmpeg\bin";
            //Library.LoadFFmpeg();

            ImagesDataView = CollectionViewSource.GetDefaultView(ImagesData) as ListCollectionView;
            ImagesDataView.CustomSort = new NaturalOrderComparer(false);
            //ImagesDataView.SortDescriptions.Add(new SortDescription { PropertyName = "ThumbnailName", Direction = ListSortDirection.Ascending });

            AppUpdater = new UpdateCheck(this);
            
            // these lines shouldn't be in this class (maybe?)
            Settings = new SettingsManager(this);
            SettingsStore.InitSettingsStore(Settings);
            ThumbnailItemData.Settings = Settings;

            ToClipboard = new CopyFileToClipboard();

            //create new watcher events for used directory
            //fsw.Changed += Fsw_Updated;
            fsw.Created += Fsw_Created;
            fsw.Deleted += Fsw_Deleted;
            fsw.Renamed += Fsw_Renamed;

            Settings.Load();

            DataContext = this;

            dragStarted = false;//hack for avoiding reloading the images when you start the program

            ProgramLoaded = true;

            //AppWindow = this;//used for debugging ZoomBorder
        }

        /*Mutex fivMutex;
        private void SingleInstance()
        {
            //dont allow more than one instance of the app
            fivMutex = new Mutex(true, "4b7ae177-8698-4713-b655-ee64c1106aea", out bool aIsNewInstance);//GUID - 4b7ae177-8698-4713-b655-ee64c1106aea
            if (!aIsNewInstance)
            {
                string[] args = Environment.GetCommandLineArgs();

                if (args.Length > 0)//get startup path
                {
                    //Path.GetDirectoryName(args[0]);
                    if (args.Length > 1)//opened file
                    {
                        OpenNewFile(args[1]);
                    }
                }

                //MessageBox.Show("Already an instance is running...");
                App.Current.Shutdown();
            }
        }

        private void ServerInstance()
        {
            try
            {
                IPAddress ipAd = IPAddress.Parse("127.0.0.1");
                // use local m/c IP address, and 
                // use the same in the client

                //Initializes the Listener
                TcpListener myList = new TcpListener(ipAd, 6969);

                //Start Listeneting at the specified port
                myList.Start();

                Console.WriteLine("The server is running at port 8001...");
                Console.WriteLine("The local End point is  :" + myList.LocalEndpoint);
                Console.WriteLine("Waiting for a connection.....");

                Socket s = myList.AcceptSocket();
                Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                byte[] b = new byte[255];
                int k = s.Receive(b);
                Console.WriteLine("Recieved...");
                for (int i = 0; i < k; i++)
                    Console.Write(Convert.ToChar(b[i]));

                b.ToString();

                ASCIIEncoding asen = new ASCIIEncoding();
                s.Send(asen.GetBytes("The string was recieved by the server."));
                Console.WriteLine("\nSent Acknowledgement");
                //clean up
                s.Close();
                myList.Stop();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }

        private void ClientInstance(string msg)
        {
            try
            {
                TcpClient tcpclnt = new TcpClient();
                Console.WriteLine("Connecting.....");

                tcpclnt.Connect("127.0.0.1", 6969);
                // use the ipaddress as in the server program

                Console.WriteLine("Connected");

                Stream stm = tcpclnt.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(msg);
                Console.WriteLine("Transmitting.....");

                stm.Write(ba, 0, ba.Length);

                byte[] bb = new byte[255];
                int k = stm.Read(bb, 0, 255);

                for (int i = 0; i < k; i++)
                    Console.Write(Convert.ToChar(bb[i]));

                tcpclnt.Close();
            }

            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }*/

        private async void OnAppLoaded(object sender, RoutedEventArgs e)
        {
            if (Settings.CheckForUpdatesStartToggle)
                await AppUpdater.CheckForUpdates(UpdateCheckType.ForcedVersionCheck);
            else
                await AppUpdater.CheckForUpdates(UpdateCheckType.SilentVersionCheck);

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 0)//get startup path
            {
                //StartupPath = Path.GetDirectoryName(args[0]);

#if DEBUG
                string path = @"D:\Google Drive\temp\alltypes\3.png";

                OpenNewFile(path);
#endif

                if (args.Length > 1)
                {
                    OpenNewFile(args[1]);
                }
            }

            /*notifier.ShowInformation("");
            notifier.ShowSuccess("");
            notifier.ShowWarning("");
            notifier.ShowError("");*/
        }

        /*private void Fsw_Updated(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string cultureChangeType;
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        cultureChangeType = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.ChangedWatcher));
                        break;
                    default:
                        cultureChangeType = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.AllWatcher));
                        break;
                }
                notifier.ShowInformation($"{cultureChangeType} \"{e.Name}\"");

                GetDirectoryFiles(ActiveFolder);

                if (ImagesData.Count < 1)
                {
                    ClearAllMedia();
                    return;
                }

                FindIndexInFiles(ActiveFile);
                //SetTitleInformation();

                ChangeImage(0, false);
            });
        }*/

        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!Tools.IsOfType(e.Name, Settings.FilterActiveArray)) return;//ignore if the file is not a valid type

                ThumbnailItemData tt = new ThumbnailItemData
                {
                    ThumbnailName = e.Name,
                    IsAnimated = Tools.IsAnimatedExtension(Path.GetExtension(e.Name).ToLower()),
                    //ThumbnailImage = GetThumbnail(e.FullPath)
                };
                ImagesData.Add(tt);

                Task.Run(() => Tools.LoadSingleThumbnailData(tt, e.FullPath, false));

                //ImagesData = ImagesData.OrderByAlphaNumeric((a) => a.ThumbnailName).ToList();//sort back changed list

                //ChangeImage(0, false);

                //FindIndexInFiles(activeFile);

                OnPropertyChanged("TitleInformation");//update title information

                notifier.ShowInformation($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.CreatedWatcher))} \"{e.Name}\"");
            });
        }

        private void Fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
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

                if (!Tools.IsOfType(e.Name, Settings.FilterActiveArray) && !dirty) return;//dont send a message if its not one of our files

                if(ImageItem == null || ImageItem.ThumbnailName == e.Name)
                {
                    ChangeImage(0, false);
                }

                OnPropertyChanged("TitleInformation");//update title information

                //FindIndexInFiles(activeFile);

                notifier.ShowInformation($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.DeletedWatcher))} \"{e.Name}\"");
            });
        }

        private void Fsw_Renamed(object sender, RenamedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    for (int i = 0; i < ImagesData.Count; i++)
                    {
                        if (e.OldName == ImagesData[i].ThumbnailName)
                        {
                            if (Tools.IsOfType(e.Name, Settings.FilterActiveArray))
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
                                    ChangeImage(0, false);
                                }

                                notifier.ShowInformation($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.RenamedWatcher))} \"{e.OldName}\" -> \"{e.Name}\"");
                            }
                            else//file has been renamed to something with a non-valid extension - remove it from the list
                            {
                                ImagesData.RemoveAt(i);
                                ChangeImage(0, false);
                            }

                            break;
                        }
                    }
                }
                catch
                {

                }
            });
        }

        public void OpenNewFile(string path)
        {
            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

            selectedNew = true;

            ActiveFile = Path.GetFileName(path);
            ActiveFolder = Path.GetDirectoryName(path);
            ActivePath = path;

            fsw.Path = ActiveFolder;
            fsw.EnableRaisingEvents = true;//File Watcher is enabled/disabled

            if (Settings.FilterActiveArray.Length == 0)
            {
                ImagesData.Clear();
                ChangeImage(0, false);
                return;
            }

            GetDirectoryFiles(ActiveFolder);

            FindIndexInFiles(activeFile);

            NewUri(path);
        }

        private void GetDirectoryFiles(string searchFolder)
        {
            ImagesData.Clear();
            List<string> filesFound = new List<string>();

            //filesFound.AddRange(Directory.GetFiles(searchFolder, "*.*", SearchOption.TopDirectoryOnly));
            //filesFound.AddRange(Directory.EnumerateFiles(searchFolder).OrderBy(filename => filename));
            //filesFound.OrderBy(p => p.Substring(0)).ToList();//probably doesnt work
            filesFound.AddRange(Directory.EnumerateFiles(searchFolder));

            int c = filesFound.Count;
            for (int i = 0; i < c; i++)
            {
                string ext = Path.GetExtension(filesFound[i].ToLower());
                if (Settings.FilterActiveArray.Any(ext.Contains))
                {
                    filesFound[i] = Path.GetFileName(filesFound[i]);

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
                if(openedFile == ((ThumbnailItemData)ImagesDataView.GetItemAt(i)).ThumbnailName)
                {
                    ImagesDataView.MoveCurrentToPosition(i);
                    //thumbnailList.SelectedIndex = ImagesDataView.CurrentPosition;

                    ActiveFile = ((ThumbnailItemData)ImagesDataView.GetItemAt(i)).ThumbnailName;
                    ActivePath = Path.Combine(ActiveFolder, activeFile);

                    break;
                }
            }
        }

        /// <summary>
        /// Clear all data (as if program is opened without opening an image)
        /// </summary>
        private void ClearAllMedia()
        {
            ImagesData.Clear();
            //MediaSource = null;
            CloseMedia();
            ImageSource = null;
            ImgWidth = 0;
            ImgHeight = 0;
        }

        private void ClearViewer()
        {
            CloseMedia();
            ImageSource = null;
            ImgWidth = 0;
            ImgHeight = 0;
        }

        private async void OpenMedia(Uri uri)
        {
            await MediaView.Open(uri);
        }

        private async void CloseMedia()
        {
            await MediaView.Close();
        }

        private void TogglePause()
        {
            if (ImageItem is null) return;

            if (ImageItem.IsAnimated)
            {
                if (MediaView.IsPaused)
                {
                    MediaView.Play();
                }
                else
                {
                    MediaView.Pause();
                }
            }
        }

        private void ChangeImage(int jump, bool moveToIndex)
        {
            if (ImagesData.Count == 0)//no more images in the folder - go back to default null
            {
                ClearAllMedia();
                return;
            }

            int jumpIndex = jump;

            if (moveToIndex)//ghost function since its already handled by ListBox and collection
            {
                //ImagesDataView.MoveCurrentToPosition(jumpIndex);
            }
            else
            {
                jumpIndex += ImagesDataView.CurrentPosition;

                //wrap around a limit between 0 and how many images there are (minus 1)
                if (jumpIndex < 0) jumpIndex = ImagesDataView.Count - 1;
                if (jumpIndex >= ImagesDataView.Count) jumpIndex = 0;

                ImagesDataView.MoveCurrentToPosition(jumpIndex);
            }

            //keep moving onward until we find an existing file
            //TEMP REPLACEMENT (maybe)
            if (!FileSystem.FileExists(Path.Combine(ActiveFolder, ((ThumbnailItemData)ImagesDataView.GetItemAt(ImagesDataView.CurrentPosition)).ThumbnailName)))
            {
                ChangeImage(jumpIndex, false);
            }

            ActiveFile = ImageItem.ThumbnailName;
            ActivePath = Path.Combine(ActiveFolder, activeFile);

            NewUri(ActivePath);
        }

        private void NewUri(string path)
        {
            if (!Tools.IsOfType(path, Settings.FilterActiveArray))
            {
                ChangeImage(0, false);
                return;
            }

            if (ImageItem.IsAnimated)
            {
                MediaProgression.Visibility = Visibility.Visible;
            }
            else
            {
                MediaProgression.Visibility = Visibility.Hidden;
            }

            if (ImageItem.IsAnimated)
            {
                borderImg.Visibility = Visibility.Hidden;
                border.Visibility = Visibility.Visible;

                Uri uri = new Uri(path, UriKind.Absolute);

                //MediaSource = uri;

                ImageSource = null;
                OpenMedia(uri);

                border.Reset();
            }
            else
            {
                borderImg.Visibility = Visibility.Visible;
                border.Visibility = Visibility.Hidden;

                if (ImagesData.Count > 0)
                {
                    Tools.GetImageInformation(ActivePath, ImageItem);
                    ImgWidth = ImageItem.ImageWidth;
                    ImgHeight = ImageItem.ImageHeight;
                }

                CloseMedia();
                ImageSource = Tools.LoadImage(path, ImgWidth, ImgHeight);

                borderImg.Reset();
            }

            selectedNew = false;

            ScrollToListView();
            GC.Collect();
        }

        CancellationTokenSource allThumbnailTokenSource;
        CancellationToken ct;

        /*public Task LoadAllThumbnailsAsync()
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

                    if(tid.ThumbnailImage is null)//dont load the thumbnail if we already have one there
                        tid.ThumbnailImage = Tools.GetThumbnail(Path.Combine(ActiveFolder, tid.ThumbnailName), tid);
                }

            }, allThumbnailTokenSource.Token);
        }*/

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

                    Tools.LoadThumbnailData(Path.Combine(ActiveFolder, tid.ThumbnailName), tid);
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

        private Task DeleteToRecycleAsync(string path)
        {
            if (!File.Exists(path)) return Task.CompletedTask;

            if(MediaView.HasVideo && forDeletiionMediaFlag == false)
            {
                IsDeletingFile = true;

                forDeletionMediaPath = ActivePath;
                forDeletiionMediaFlag = true;
                CloseMedia();
            }

            if (forDeletiionMediaFlag == true) return Task.CompletedTask;

            return Task.Run(() =>
            {
                IsDeletingFile = true;

                if (FileSystem.FileExists(path))
                {
                    FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        notifier.ShowWarning($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.FileNotFoundMsg))}: {path}");
                    });
                }

                IsDeletingFile = false;
                forDeletiionMediaFlag = false;
            });
        }

        private void ImageCopyToClipboardCall()
        {
            if (ImageItem is null) return;

            string fileType = Path.GetExtension(ImageItem.ThumbnailName);

            if (!File.Exists(ActivePath) || fileType == ".webm") return;

            if (ImageItem.IsAnimated)
            {
                //ToClipboard.GifCopyToClipboard(MediaSource);
                ToClipboard.ImageCopyToClipboard(new BitmapImage(MediaView.Source));
            }
            else
            {
                ToClipboard.ImageCopyToClipboard(ImageSource);
            }

            notifier.ShowWarning($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.CopiedToClipboard))}\n\"{ActivePath}\"");
        }

        /*private void FileCopyToClipboardCall()
        {
            ToClipboard.FileCopyToClipboard(ActivePath);
        }*/

        private void FileCutToClipboardCall()
        {
            if (ImageItem is null || !File.Exists(ActivePath)) return;

            string fileType = Path.GetExtension(ImageItem.ThumbnailName);
            if (fileType == ".gif"  || fileType == ".webm")//TODO: temp fix
            {
                ClearViewer();
            }

            ToClipboard.FileCutToClipBoard(ActivePath);

            notifier.ShowWarning($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.CutToClipboard))}\n\"{ActivePath}\"");
        }

        #region XAML events
        private void Media_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                if (files.Length >= 1)
                    OpenNewFile(files[0]);
            }
        }

        private void OnDonateClick(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo(DonationLink);
            Process.Start(sInfo);
        }

        private void MediaView_MediaOpened(object sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            if (ImagesData.Count > 0)
            {
                ImgWidth = MediaView.NaturalVideoWidth;
                ImgHeight = MediaView.NaturalVideoHeight;

                ImageItem.ImageWidth = ImgWidth;
                ImageItem.ImageHeight = ImgHeight;

                totalMediaTime = MediaView.NaturalDuration.Value.TotalMilliseconds;
            }
        }

        private void MediaView_PositionChanged(object sender, Unosquare.FFME.Common.PositionChangedEventArgs e)
        {
            double currentTime = e.Position.TotalMilliseconds;
            
            if (totalMediaTime == 0 || currentTime == 0)
                MediaProgression.Value = 0;
            else
                MediaProgression.Value = (currentTime / totalMediaTime) * 100;
        }

        private void MediaView_MediaClosed(object sender, EventArgs e)
        {
            if (forDeletiionMediaFlag == true)
            {
                forDeletiionMediaFlag = false;
                DeleteToRecycleAsync(forDeletionMediaPath);
            }

            GC.Collect();//clean up memory just in case
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
        }

        private void OnAccentClick(object sender, RoutedEventArgs e)
        {
            if (Settings.ThemeAccentDropIndex >= Settings.ThemeAccents.Count - 1)
                Settings.ThemeAccentDropIndex = 0;
            else
                Settings.ThemeAccentDropIndex++;

            ThemeAccentDrop.SelectedIndex = Settings.ThemeAccentDropIndex;
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
            //IInputElement focusedControl = FocusManager.GetFocusedElement(this);
            //MessageBox.Show(focusedControl.ToString());

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
                selectedNew = true;
                ChangeImage(1, false);//go forward
            }
            if (e.Key == Settings.GoBackwardKey)
            {
                selectedNew = true;
                ChangeImage(-1, false);//go back
            }

            if (e.Key == Settings.PauseKey)
            {
                TogglePause();
            }

            if (e.Key == Settings.DeleteKey && ImagesData.Count > 0)
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

            if(e.Key == Settings.ThumbnailListKey)
            {
                Settings.EnableThumbnailListToggle = !Settings.EnableThumbnailListToggle;
            }
        }

        private void OnClick_Prev(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

            selectedNew = true;
            ChangeImage(-1, false);//go back
        }

        private void OnClick_Next(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

            selectedNew = true;
            ChangeImage(1, false);//go forward
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsDeletingFile || Settings.ShortcutButtonsOn == false) return;

            if (e.ChangedButton == MouseButton.XButton1)
            {
                selectedNew = true;
                ChangeImage(-1, false);//go back
            }
            if (e.ChangedButton == MouseButton.XButton2)
            {
                selectedNew = true;
                ChangeImage(1, false);//go forward
            }
        }

        private void OnOpenBrowseImage(object sender, RoutedEventArgs e)
        {
            if (isDeletingFile || Settings.ShortcutButtonsOn == false) return;

            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                OpenNewFile(openFileDialog.FileName);
            }
            /*else
            {
                //cancelled dialog
            }*/
        }

        private void OnShortcutClick(object sender, RoutedEventArgs e)
        {
            editingButton = (Button)sender;

            Settings.ShortcutButtonsOn = false;//disable the buttons until done editing

            //TODO: put text when editing for user to know; save changed buttons
        }

        private void OnRemoveShortcutClick(object sender, RoutedEventArgs e)
        {
            editingButton = (Button)sender;

            editingButton.Tag = Key.None;//set shortcut to none

            Settings.UpdateAllKeysProperties();
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

        private void ThumbnailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImagesDataView.CurrentPosition < 0) return;

            if (!selectedNew)
            {
                ChangeImage(0, true);
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

        private void MetroTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Settings.ReloadFolderFlag == false) return;//dont refresh if not flagged

            Settings.ReloadFolderFlag = false;
            OpenNewFile(ActivePath);
        }

        private void OnThumbnailItemVisible(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbi = sender as ListBoxItem;
            ThumbnailItemData dataItem = (ThumbnailItemData)lbi.Content;

            if (Path.GetExtension(dataItem.ThumbnailName) == ".webm")
            {
                try
                {
                    var shellFile = ShellObject.FromParsingName(Path.Combine(ActiveFolder, dataItem.ThumbnailName));
                    dataItem.ThumbnailImage = shellFile.Thumbnail.BitmapSource;
                    shellFile.Dispose();
                }
                catch
                {

                }
            }
            else
                Task.Run(() => Tools.LoadSingleThumbnailData(dataItem, Path.Combine(ActiveFolder, dataItem.ThumbnailName), false));
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo(e.Uri.ToString());
            Process.Start(sInfo);
        }

        private void MainFIV_Closing(object sender, CancelEventArgs e)
        {
            fsw.Created -= Fsw_Created;
            fsw.Deleted -= Fsw_Deleted;
            fsw.Renamed -= Fsw_Renamed;

            ClearAllMedia();
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