﻿using FIVStandard.Comparers;
using FIVStandard.Core;
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
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

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
        private Uri mediaSource = null;

        public Uri MediaSource
        {
            get
            {
                return mediaSource;
            }
            set
            {
                mediaSource = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage imageSource = null;

        public BitmapImage ImageSource
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
                OnPropertyChanged("ImgResolution");
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
                OnPropertyChanged("ImgResolution");
            }
        }

        public string ImgResolution
        {
            get
            {
                if (ImgWidth == 0 || ImgHeight == 0)
                    return "owo";
                else
                    return $"{ImgWidth}x{ImgHeight}";
            }
        }

        public Rotation ImageRotation { get; set; } = Rotation.Rotate0;
        #endregion

        public UpdateCheck AppUpdater { get; set; }

        public SettingsManager Settings { get; set; }

        public CopyFileToClipboard ToClipboard { get; set; }

        public string StartupPath;//program startup path

        private bool selectedNew = false;//used to avoid ListBox event to re-select the image, doubling the loading time

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        private readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico", ".webp"/*, ".tiff", ".svg", ".mp4", ".avi" */ };//TODO: doesnt work: tiff svg
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Images|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.ICO;*.WEBP"/* + "|All files (*.*)|*.*" */};

        private Button editingButton = null;//used for editing shortcuts

        private bool IsPaused { get; set; } = false;//if the animated image (gif) is paused or not

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

            ImagesDataView = CollectionViewSource.GetDefaultView(ImagesData) as ListCollectionView;
            //ImagesDataView.SortDescriptions.Add(new SortDescription { PropertyName = "ThumbnailName", Direction = ListSortDirection.Ascending });
            ImagesDataView.CustomSort = new NaturalOrderComparer(false);

            AppUpdater = new UpdateCheck(this);
            
            // these lines shouldn't be in this class
            Settings = new SettingsManager(this);
            SettingsStore.InitSettingsStore(Settings);

            ToClipboard = new CopyFileToClipboard();

            ThumbnailItemData.Settings = Settings;

            //create new watcher events for used directory
            //fsw.Changed += Fsw_Updated;
            fsw.Created += Fsw_Created;
            fsw.Deleted += Fsw_Deleted;
            fsw.Renamed += Fsw_Renamed;

            DataContext = this;

            Settings.Load();

            dragStarted = false;//hack for avoiding reloading the images when you start the program

            //AppWindow = this;//used for debugging ZoomBorder
        }

        private void OnAppLoaded(object sender, RoutedEventArgs e)
        {
            if (Settings.CheckForUpdatesStartToggle)
                AppUpdater.CheckForUpdates(UpdateCheckType.ForcedVersionCheck);
            else
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
                ThumbnailItemData tt = new ThumbnailItemData
                {
                    ThumbnailName = e.Name,
                    IsAnimated = Tools.IsAnimatedExtension(Path.GetExtension(e.Name)),
                    //ThumbnailImage = GetThumbnail(e.FullPath)
                };
                ImagesData.Add(tt);

                Task.Run(() => Tools.LoadSingleThumbnail(tt, e.FullPath, false));

                //ImagesData = ImagesData.OrderByAlphaNumeric((a) => a.ThumbnailName).ToList();//sort back changed list

                ChangeImage(0, false);

                FindIndexInFiles(activeFile);

                notifier.ShowInformation($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.CreatedWatcher))} \"{e.Name}\"");
            });
        }

        private void Fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < ImagesData.Count; i++)
                {
                    if (e.Name == ImagesData[i].ThumbnailName)
                    {
                        ImagesData.RemoveAt(i);

                        break;
                    }
                }

                ChangeImage(0, false);

                FindIndexInFiles(activeFile);

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
                            BitmapImage oldThumbnail = ImagesData[i].ThumbnailImage;//save the thumbnail so we dont have to generate it again

                            ImagesData.RemoveAt(i);

                            ThumbnailItemData tt = new ThumbnailItemData
                            {
                                ThumbnailName = e.Name,
                                IsAnimated = Tools.IsAnimatedExtension(Path.GetExtension(e.Name)),

                                ThumbnailImage = oldThumbnail//just replace with the old thumbnail to save performance
                            };
                            ImagesData.Add(tt);

                            //LoadSingleThumbnail(e.Name, e.FullPath, false);

                            //ImagesData = ImagesData.OrderByAlphaNumeric((a) => a.ThumbnailName).ToList();//sort back changed list

                            //if the viewed item is the changed one, update it
                            if (activeFile == e.OldName)
                            {
                                ActiveFile = ImagesData[i].ThumbnailName;
                            }

                            ChangeImage(0, false);

                            FindIndexInFiles(activeFile);

                            break;
                        }
                    }

                    notifier.ShowInformation($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.RenamedWatcher))} \"{e.OldName}\" -> \"{e.Name}\"");
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
                if (filters.Any(Path.GetExtension(filesFound[i].ToLower()).Contains))
                {
                    filesFound[i] = Path.GetFileName(filesFound[i]);

                    ThumbnailItemData tt = new ThumbnailItemData
                    {
                        ThumbnailName = filesFound[i],
                        IsAnimated = Tools.IsAnimatedExtension(Path.GetExtension(filesFound[i])),
                    };
                    ImagesData.Add(tt);
                }
            }

            LoadAllThumbnailsAsync();
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
            MediaSource = null;
            ImageSource = null;
            ImgWidth = 0;
            ImgHeight = 0;
        }

        private void TogglePause()
        {
            if (ImageItem is null) return;

            if (ImageItem.IsAnimated)
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

            //ImageItem = ((ThumbnailItemData)ImagesDataView.GetItemAt(jumpIndex));

            //TODO: fix random removes if deleting and adding back files
            /*if (!FileSystem.FileExists(Path.Combine(ActiveFolder, ActiveFile)))//keep moving onward until we find an existing file
            {
                //remove nonexistent file from list - if there are more than 1
                if (ImagesData.Count > 1)
                {
                    ImagesData.RemoveAt(ImagesDataView.CurrentPosition);
                    Console.WriteLine($"ChangeImage- REMOVED ITEM {ActiveFile}@{ImagesDataView.CurrentPosition}");
                    //SetTitleInformation();
                }

                ChangeImage(jumpIndex, false);

                return;
            }*/

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
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();//DEBUG
            stopwatch.Start();//DEBUG
#endif

            if (ImageItem.IsAnimated)
            {
                borderImg.Visibility = Visibility.Hidden;
                border.Visibility = Visibility.Visible;

                IsPaused = false;

                Uri uri = new Uri(path, UriKind.Absolute);

                MediaSource = uri;
                ImageSource = null;

                MediaView.Play();
                border.Reset();
            }
            else
            {
                borderImg.Visibility = Visibility.Visible;
                border.Visibility = Visibility.Hidden;

                if (ImagesData.Count > 0)
                {
                    var (iWidth, iHeight, iRotation) = Tools.GetImageInformation(ActivePath, ImageItem);
                    ImgWidth = iWidth;
                    ImgHeight = iHeight;
                    ImageRotation = iRotation;
                }

                MediaSource = null;
                ImageSource = Tools.LoadImage(path, ImgWidth, ImgHeight, ImageRotation);

                borderImg.Reset();
            }

            selectedNew = false;

#if DEBUG
            stopwatch.Stop();//DEBUG
            notifier.ShowError($"NewUri time: {stopwatch.ElapsedMilliseconds}ms");//DEBUG
#endif

            ScrollToListView();
        }

        CancellationTokenSource allThumbnailTokenSource;
        CancellationToken ct;

        public Task LoadAllThumbnailsAsync()
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

                    /*if (tid.IsAnimated)//TODO add option "Animate thumbnail gifs" check
                    {
                        //tid.ThumbnailMedia = new Uri(Path.Combine(ActiveFolder, tid.ThumbnailName));
                    }
                    else
                    {
                        //TODO put normal thumbnail load here
                    }*/

                    if(tid.ThumbnailImage is null)//dont load the thumbnail if we already have one there
                        tid.ThumbnailImage = Tools.GetThumbnail(Path.Combine(ActiveFolder, tid.ThumbnailName), tid);
                }

            }, allThumbnailTokenSource.Token);
        }

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

                    /*if (tid.IsAnimated)//TODO add option "Animate thumbnail gifs" check
                    {
                        //tid.ThumbnailMedia = new Uri(Path.Combine(ActiveFolder, tid.ThumbnailName));
                    }
                    else
                    {
                        //TODO put normal thumbnail load here
                    }*/

                    tid.ThumbnailImage = Tools.GetThumbnail(Path.Combine(ActiveFolder, tid.ThumbnailName), tid);
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

            return Task.Run(() =>
            {
                IsDeletingFile = true;

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
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        notifier.ShowWarning($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.FileNotFoundMsg))}: {path}");
                    });
                }

                IsDeletingFile = false;
            });
        }

        private void ImageCopyToClipboardCall()
        {
            //ToClipboard.CopyToClipboard(ActivePath);
            if (ImageItem is null || !File.Exists(ActivePath)) return;

            if (ImageItem.IsAnimated)
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
            if (ImageItem is null || !File.Exists(ActivePath)) return;

            ToClipboard.FileCutToClipBoard(ActivePath);
        }

        #region XAML events
        private void OnDonateClick(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6ZXTCHB3JXL4Q&source=url");
            Process.Start(sInfo);
        }

        private void OnClipEnded(object sender, RoutedEventArgs e)
        {
            MediaView.Position = new TimeSpan(0, 0, 1);
            MediaView.Play();
        }

        private void OnClipOpened(object sender, RoutedEventArgs e)
        {
            if (ImagesData.Count > 0)
            {
                var (iWidth, iHeight, iRotation) = Tools.GetImageInformation(ActivePath, ImageItem);
                ImgWidth = iWidth;
                ImgHeight = iHeight;
                ImageRotation = iRotation;
            }
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

            //GC.Collect();
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

            editingButton.Tag = Key.None;

            Settings.UpdateAllKeysProperties();
        }

        private void OnResetSettingsClick(object sender, RoutedEventArgs e)
        {
            Settings.ResetToDefault();
        }

        private void OnCheckUpdateClick(object sender, RoutedEventArgs e)
        {
            AppUpdater.CheckForUpdates(UpdateCheckType.ForcedVersionCheck);
        }

        private void OnForceDownloadSetupClick(object sender, RoutedEventArgs e)
        {
            AppUpdater.CheckForUpdates(UpdateCheckType.FullUpdateForced);
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

            if (cp < 0)//dont do anything if not 0 or more aka nothing selected (-1)
                return;

            thumbnailList.ScrollIntoView(ImagesDataView.GetItemAt(cp));
        }

        //select event when using the mouse on the list box items
        private void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var tid = (ListBoxItem)sender;
            ImageItem = (ThumbnailItemData)tid.Content;
        }

        private bool dragStarted = true;

        private void ThumbnailSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ReloadAllThumbnailsAsync();
            this.dragStarted = false;
        }

        private void ThumbnailSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.dragStarted = true;
        }

        public void ThumbnailResSlider_ValueChanged()
        {
            if (!dragStarted)
                ReloadAllThumbnailsAsync();
        }

        /*private void ThumbnailMedia_OnClipEnded(object sender, RoutedEventArgs e)//used for list box's data template's media element control
        {
            MediaElement me = (MediaElement)sender;
            me.Position = new TimeSpan(0, 0, 1);
            me.Play();
        }*/
        #endregion

        /*private int ParseStringToOnlyInt(string input)
        {
            return int.Parse(string.Join("", input.Where(x => char.IsDigit(x))));
        }*/

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
