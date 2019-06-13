using Caliburn.Micro;
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
using System.Windows.Media.Imaging;

namespace FIVStandard.ViewModels
{
    public class MainViewModel : Screen
    {
<<<<<<< HEAD
<<<<<<< HEAD
        
=======
=======
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
        public int ImageIndex { get; set; } = 0;

        private bool isPaused = false;

        public int ImgWidth { get; set; } = 0;
        public int ImgHeight { get; set; } = 0;

        //private string startupPath;//program startup path

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

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

        public bool IsDeletingFile { get; private set; } = false;

<<<<<<< HEAD
<<<<<<< HEAD
        private int themeAccentDropIndex = 0;
=======
        private int _themeAccentDropIndex = 0;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private int themeAccentDropIndex = 0;
>>>>>>> parent of 59af935... Last MVVM before removal

        public int ThemeAccentDropIndex
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return themeAccentDropIndex;
            }
            set
            {
                themeAccentDropIndex = value;
=======
                return _themeAccentDropIndex;
            }
            set
            {
                _themeAccentDropIndex = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return themeAccentDropIndex;
            }
            set
            {
                themeAccentDropIndex = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => ThemeAccentDropIndex);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======

>>>>>>> parent of 2cbf28d... semi-working mvvm, downsize broken
        private bool darkModeToggle = true;
=======
        private bool _darkModeToggle = true;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private bool darkModeToggle = true;
>>>>>>> parent of 59af935... Last MVVM before removal

        public bool DarkModeToggle
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return darkModeToggle;
            }
            set
            {
                darkModeToggle = value;
=======
                return _darkModeToggle;
            }
            set
            {
                _darkModeToggle = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return darkModeToggle;
            }
            set
            {
                darkModeToggle = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => DarkModeToggle);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        private bool stretchImageToggle = true;
=======
        private bool _stretchImageToggle = true;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private bool stretchImageToggle = true;
>>>>>>> parent of 59af935... Last MVVM before removal

        public bool StretchImageToggle
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return stretchImageToggle;
            }
            set
            {
                stretchImageToggle = value;
=======
                return _stretchImageToggle;
            }
            set
            {
                _stretchImageToggle = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return stretchImageToggle;
            }
            set
            {
                stretchImageToggle = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => StretchImageToggle);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        private bool downsizeImageToggle = false;
=======
        private bool _downsizeImageToggle = false;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private bool downsizeImageToggle = false;
>>>>>>> parent of 59af935... Last MVVM before removal

        public bool DownsizeImageToggle
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return downsizeImageToggle;
            }
            set
            {
                downsizeImageToggle = value;
=======
                return _downsizeImageToggle;
            }
            set
            {
                _downsizeImageToggle = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return downsizeImageToggle;
            }
            set
            {
                downsizeImageToggle = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => DownsizeImageToggle);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======

>>>>>>> parent of 2cbf28d... semi-working mvvm, downsize broken
        private string windowTitle = "FIV";
=======
        private string _windowTitle = "FIV";
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private string windowTitle = "FIV";
>>>>>>> parent of 59af935... Last MVVM before removal

        public string WindowTitle
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return windowTitle;
            }
            set
            {
                windowTitle = value;
=======
                return _windowTitle;
            }
            set
            {
                _windowTitle = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return windowTitle;
            }
            set
            {
                windowTitle = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => WindowTitle);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        private double zoomSensitivity = 0.2;
=======
        private double _zoomSensitivity = 0.2;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private double zoomSensitivity = 0.2;
>>>>>>> parent of 59af935... Last MVVM before removal

        public double ZoomSensitivity
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return zoomSensitivity;
            }
            set
            {
                zoomSensitivity = value;
=======
                return _zoomSensitivity;
            }
            set
            {
                _zoomSensitivity = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return zoomSensitivity;
            }
            set
            {
                zoomSensitivity = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => ZoomSensitivity);
            }
        }

        public string ZoomSensitivityString
        {
            get
            {
                return $"{ZoomSensitivity}";
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        private Uri mediaSource = null;
=======
        private Uri _mediaSource = null;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private Uri mediaSource = null;
>>>>>>> parent of 59af935... Last MVVM before removal

        public Uri MediaSource
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return mediaSource;
            }
            set
            {
                mediaSource = value;
=======
                return _mediaSource;
            }
            set
            {
                _mediaSource = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return mediaSource;
            }
            set
            {
                mediaSource = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => MediaSource);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        private BitmapImage imageSource = null;
=======
        private BitmapImage _imageSource = null;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private BitmapImage imageSource = null;
>>>>>>> parent of 59af935... Last MVVM before removal

        public BitmapImage ImageSource
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return imageSource;
            }
            set
            {
                imageSource = value;
=======
                return _imageSource;
            }
            set
            {
                _imageSource = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return imageSource;
            }
            set
            {
                imageSource = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => ImageSource);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
        private Visibility borderImgVisibility = Visibility.Visible;
=======
        private Visibility _borderImgVisibility = Visibility.Visible;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private Visibility borderImgVisibility = Visibility.Visible;
>>>>>>> parent of 59af935... Last MVVM before removal
=======
        private Visibility borderImgVisibility = Visibility.Hidden;
>>>>>>> parent of 2cbf28d... semi-working mvvm, downsize broken

        public Visibility BorderImgVisible
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return borderImgVisibility;
            }
            set
            {
                borderImgVisibility = value;
=======
                return _borderImgVisibility;
            }
            set
            {
                _borderImgVisibility = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return borderImgVisibility;
            }
            set
            {
                borderImgVisibility = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => BorderImgVisible);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        private Visibility borderMediaVisible = Visibility.Hidden;
=======
        private Visibility _borderMediaVisible = Visibility.Hidden;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private Visibility borderMediaVisible = Visibility.Hidden;
>>>>>>> parent of 59af935... Last MVVM before removal

        public Visibility BorderMediaVisible
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return borderMediaVisible;
            }
            set
            {
                borderMediaVisible = value;
=======
                return _borderMediaVisible;
            }
            set
            {
                _borderMediaVisible = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return borderMediaVisible;
            }
            set
            {
                borderMediaVisible = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => BorderMediaVisible);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        private double borderImageWidth;
=======
        private double _borderImageWidth;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private double borderImageWidth;
>>>>>>> parent of 59af935... Last MVVM before removal

        public double BorderImageWidth
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return borderImageWidth;
            }
            set
            {
                borderImageWidth = value;
=======
                return _borderImageWidth;
            }
            set
            {
                _borderImageWidth = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return borderImageWidth;
            }
            set
            {
                borderImageWidth = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => BorderImageWidth);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
        private double borderImageHeight;
=======
        private double _borderImageHeight;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private double borderImageHeight;
>>>>>>> parent of 59af935... Last MVVM before removal

        public double BorderImageHeight
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return borderImageHeight;
            }
            set
            {
                borderImageHeight = value;
=======
                return _borderImageHeight;
            }
            set
            {
                _borderImageHeight = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return borderImageHeight;
            }
            set
            {
                borderImageHeight = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => BorderImageHeight);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> parent of 2cbf28d... semi-working mvvm, downsize broken
        private string imageInfo;
=======
        private string _imageInfo;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private string imageInfo;
>>>>>>> parent of 59af935... Last MVVM before removal

        public string ImageInfo
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return imageInfo;
            }
            set
            {
                imageInfo = value;
=======
                return _imageInfo;
            }
            set
            {
                _imageInfo = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return imageInfo;
            }
            set
            {
                imageInfo = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => ImageInfo);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        private bool settingsFlyout = false;
=======
        private bool _settingsFlyout = false;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private bool settingsFlyout = false;
>>>>>>> parent of 59af935... Last MVVM before removal

        public bool SettingsFlyout
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return settingsFlyout;
            }
            set
            {
                settingsFlyout = value;
=======
                return _settingsFlyout;
            }
            set
            {
                _settingsFlyout = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return settingsFlyout;
            }
            set
            {
                settingsFlyout = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => SettingsFlyout);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        private bool helpFlyout = false;
=======
        private bool _helpFlyout = false;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private bool helpFlyout = false;
>>>>>>> parent of 59af935... Last MVVM before removal

        public bool HelpFlyout
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return helpFlyout;
            }
            set
            {
                helpFlyout = value;
=======
                return _helpFlyout;
            }
            set
            {
                _helpFlyout = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return helpFlyout;
            }
            set
            {
                helpFlyout = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => HelpFlyout);
            }
        }

<<<<<<< HEAD
<<<<<<< HEAD
        private StretchDirection imageViewStretchDir = StretchDirection.DownOnly;
=======
        private StretchDirection _imageViewStretchDir = StretchDirection.DownOnly;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
        private StretchDirection imageViewStretchDir = StretchDirection.DownOnly;
>>>>>>> parent of 59af935... Last MVVM before removal

        public StretchDirection ImageViewStretchDir
        {
            get
            {
<<<<<<< HEAD
<<<<<<< HEAD
                return imageViewStretchDir;
            }
            set
            {
                imageViewStretchDir = value;
=======
                return _imageViewStretchDir;
            }
            set
            {
                _imageViewStretchDir = value;
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
=======
                return imageViewStretchDir;
            }
            set
            {
                imageViewStretchDir = value;
>>>>>>> parent of 59af935... Last MVVM before removal
                NotifyOfPropertyChange(() => ImageViewStretchDir);
            }
        }
        private readonly FileLoaderModel fileLoader;

        public MainViewModel()
        {
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
            windowTitle = $"[{ImageIndex + 1}/{fileLoader.ImagesFound.Count}] {Path.GetFileName(fileLoader.ImagesFound[ImageIndex])}";
        }

        /// <summary>
        /// Clear all saved paths and clean media view and finally cleanup memory
        /// </summary>
        private void ClearAllMedia()
        {
            fileLoader.ImagesFound.Clear();
            mediaSource = null;
            ImageSource = null;
            windowTitle = "FIV";

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

                imageInfo = $"{ImgWidth}x{ImgHeight}";
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
            /*controller = ImageBehavior.GetAnimationController(MainImage);

            if (!isAnimated) return;

            if (controller.IsPaused)
                controller.Play();
            else
                controller.Pause();*/

            if (fileLoader.IsAnimated)
            {
                if (isPaused)
                {
                    //MediaView.Play();//TODO
                    isPaused = false;
                }
                else
                {
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

                //MediaView.Play();//TODO
                //border.Reset();//TODO
            }
            else
            {
                //borderImg.Reset();//TODO
            }

            //MainImage.Source = bm;
        }

        public void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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

            if (e.Key == System.Windows.Input.Key.Delete && fileLoader.ImagesFound.Count > 0)
            {
                DeleteToRecycle(fileLoader.ImagesFound[ImageIndex]);
            }

            if (e.Key == System.Windows.Input.Key.F)
            {
                stretchImageToggle = !stretchImageToggle;
            }

            if (e.Key == System.Windows.Input.Key.E)
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

        public void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        public void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            helpFlyout = false;
            settingsFlyout = !settingsFlyout;
        }

        public void OnHelpClick(object sender, RoutedEventArgs e)
        {
            settingsFlyout = false;
            helpFlyout = !helpFlyout;
        }

        public void OnCloseFlyoutsClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            settingsFlyout = false;
            helpFlyout = false;
        }

        public void OnDonateClick(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6ZXTCHB3JXL4Q&source=url");
            Process.Start(sInfo);
        }

        private void LoadAllSettings()
        {
            //Theme
            darkModeToggle = Properties.Settings.Default.DarkTheme;
            ChangeTheme(Properties.Settings.Default.ThemeAccent);
            //Accent
            //ThemeAccentDrop.ItemsSource = themeAccents;//init for theme list
            themeAccentDropIndex = Properties.Settings.Default.ThemeAccent;
            //Image Stretch
            stretchImageToggle = Properties.Settings.Default.ImageStretched;
            ChangeStretch();
            //Downsize Images
            downsizeImageToggle = Properties.Settings.Default.DownsizeImage;
            //Zoom Sensitivity
            zoomSensitivity = Properties.Settings.Default.ZoomSensitivity;
            ChangeZoomSensitivity();
            //ZoomSensitivitySlider.ValueChanged += OnZoomSensitivitySlider;//TODO

            //ChangeAccent();//not needed since we calling ChangeTheme in there
        }

        public void OnThemeSwitch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DarkTheme = !Properties.Settings.Default.DarkTheme;
            ChangeTheme(Properties.Settings.Default.ThemeAccent);
        }

        private void ChangeTheme(int themeIndex)
        {
            if (darkModeToggle)
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[themeIndex]), ThemeManager.GetAppTheme("BaseDark"));
            }
            else
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[themeIndex]), ThemeManager.GetAppTheme("BaseLight"));
            }

            Properties.Settings.Default.Save();
        }

        public void OnAccentChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.ThemeAccent = themeAccentDropIndex;
            ChangeAccent();
        }

        private void ChangeAccent()
        {
            ChangeTheme(Properties.Settings.Default.ThemeAccent);//since theme also is rooted with accent
        }

        public void OnAccentClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ThemeAccent++;

            if (Properties.Settings.Default.ThemeAccent > ThemeAccents.Count - 1)
                Properties.Settings.Default.ThemeAccent = 0;

            themeAccentDropIndex = Properties.Settings.Default.ThemeAccent;

            //ChangeAccent();//called in OnAccentChanged
        }

        public void OnStretchSwitch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ImageStretched = !Properties.Settings.Default.ImageStretched;
            ChangeStretch();
        }

        private void ChangeStretch()
        {
            if (stretchImageToggle)
            {
                imageViewStretchDir = StretchDirection.Both;
            }
            else
            {
                imageViewStretchDir = StretchDirection.DownOnly;
            }

            Properties.Settings.Default.Save();
        }

        public void OnOpenFileLocation(object sender, RoutedEventArgs e)
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

        public void OnDeleteClick(object sender, RoutedEventArgs e)
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
                        windowTitle = "Deleting " + Path.GetFileName(path) + "...";
                    });

                    if (FileSystem.FileExists(path))
                    {
                        FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
                        //remove removed item from list

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            fileLoader.ImagesFound.RemoveAt(ImageIndex);
                            ChangeImage(-1);//go back to a previous file after deletion
                            //SetTitleInformation();
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

        public void OnDownsizeSwitch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DownsizeImage = !Properties.Settings.Default.DownsizeImage;
            ChangeDownsize();
        }

        private void ChangeDownsize()
        {
            imageSource = fileLoader.LoadImage(new Uri(fileLoader.ImagesFound[ImageIndex], UriKind.Absolute));

            Properties.Settings.Default.Save();
        }

        public void OnZoomSensitivitySlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Properties.Settings.Default.ZoomSensitivity = zoomSensitivity;
            ChangeZoomSensitivity();
        }

        private void ChangeZoomSensitivity()
        {
            //zoomSensitivity = Properties.Settings.Default.ZoomSensitivity;
            //ZoomSensitivity.Text = zoomSensitivity.ToString();

            Properties.Settings.Default.Save();
        }
<<<<<<< HEAD
>>>>>>> parent of 59af935... Last MVVM before removal
=======
>>>>>>> parent of f19e50d... failed attempt to remove mvvm
    }
}
