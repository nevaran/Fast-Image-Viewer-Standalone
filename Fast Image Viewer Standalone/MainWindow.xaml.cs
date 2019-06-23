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

namespace FIVStandard
{
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private int ImageIndex = 0;
        private List<string> ImagesFound { get; set; } = new List<string>();

        private bool IsAnimated = false;
        private bool IsPaused = false;

        //private int ImgWidth = 0;
        //private int ImgHeight = 0;

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
                return $"{imgWidth}x{imgHeight}";
            }
        }

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

                OnDownsizeSwitch();
            }
        }

        private double zoomSensitivity = 0.2;

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

                OnZoomSensitivitySlider();
            }
        }

        public string ZoomSensitivityString
        {
            get
            {
                return ZoomSensitivity.ToString("0.##");
            }
        }
        #endregion

        //private string StartupPath;//program startup path

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        private readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif"/*, ".tiff"*/, ".bmp"/*, ".svg"*/, ".ico"/*, ".mp4", ".avi" */, ".JPG", ".JPEG", ".GIF", ".BMP", ".ICO", ".PNG"};//TODO: doesnt work: tiff svg
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Images (*.JPG, *.JPEG, *.PNG, *.GIF, *.BMP, *ICO)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.ICO"/* + "|All files (*.*)|*.*" */};

        private bool IsDeletingFile = false;

        public MainWindow()
        {
            InitializeComponent();

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
                //string path = "D:\\Google Drive\\temp\\qmrns28.gif";

                //OpenNewFile(path);
#endif
            }

            if (args.Length > 1)
            {
                OpenNewFile(args[1]);
            }
        }

        public void OpenNewFile(string path)
        {
            if (IsDeletingFile) return;

            GetDirectoryFiles(Path.GetDirectoryName(path));

            FindIndexInFiles(path);
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
                    ImagesFound.Add(filesFound[i]);
                }
            }

            ImagesFound.Sort(new NameComparer());
        }

        private void FindIndexInFiles(string openedPathFile)
        {
            int L = ImagesFound.Count;
            for (int i = 0; i < L; i++)
            {
                if(openedPathFile == ImagesFound[i])
                {
                    ImageIndex = i;
                    //MessageBox.Show(imagesFound.Count + " | " + imageIndex);//DEBUG
                    break;
                }
            }
        }

        private void SetTitleInformation()
        {
            this.Title = $"[{ImageIndex + 1}/{ImagesFound.Count}] {Path.GetFileName(ImagesFound[ImageIndex])}";
        }

        /// <summary>
        /// Clear all saved paths and clean media view and finally cleanup memory
        /// </summary>
        private void ClearAllMedia()
        {
            ImagesFound.Clear();
            MediaSource = null;
            ImageSource = null;
            //ImageInfoText.Text = string.Empty;
            this.Title = "FIV";

            //GC.Collect();
        }

        private void OnClipOpened(object sender, RoutedEventArgs e)
        {
            if (ImagesFound.Count == 0) return;

            using (var imageStream = File.OpenRead(ImagesFound[ImageIndex]))
            {
                var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                ImgWidth = decoder.Frames[0].PixelWidth;
                ImgHeight = decoder.Frames[0].PixelHeight;

                //ImageInfoText.Text = $"{ImgWidth}x{ImgHeight}";
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

            if (!FileSystem.FileExists(ImagesFound[ImageIndex]))//keep moving onward until we find an existing file
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

            NewUri(ImagesFound[ImageIndex]);

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

        private void ImageChanged()
        {
            if (IsAnimated)
            {
                IsPaused = false;

                MediaView.Play();
                border.Reset();
            }
            else
            {
                borderImg.Reset();
            }

            //MainImage.Source = bm;
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
            }
            else
            {
                borderImg.Visibility = Visibility.Visible;
                border.Visibility = Visibility.Hidden;

                OnClipOpened(null, null);

                //MediaView?.Close();
                MediaSource = null;
                ImageSource = LoadImage(uri);
            }
            
            ImageChanged();

            //GC.Collect();
        }

        private BitmapImage LoadImage(Uri uri)
        {
            BitmapImage imgTemp = new BitmapImage();
            imgTemp.BeginInit();
            imgTemp.CacheOption = BitmapCacheOption.OnLoad;
            imgTemp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            imgTemp.UriSource = uri;
            if (Properties.Settings.Default.DownsizeImage)
            {
                if(ImgWidth > borderImg.ActualWidth)
                    imgTemp.DecodePixelWidth = (int)borderImg.ActualWidth;
                else if(ImgHeight > borderImg.ActualHeight)
                    imgTemp.DecodePixelHeight = (int)borderImg.ActualHeight;
            }
            imgTemp.EndInit();
            imgTemp.Freeze();

            return imgTemp;
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsDeletingFile) return;

            if(e.Key == System.Windows.Input.Key.Right)
            {
                ChangeImage(1);//go forward
            }
            if(e.Key == System.Windows.Input.Key.Left)
            {
                ChangeImage(-1);//go back
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                TogglePause();
            }

            if (e.Key == System.Windows.Input.Key.Delete && ImagesFound.Count > 0)
            {
                DeleteToRecycleAsync(ImagesFound[ImageIndex]);
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

            if(e.ChangedButton == System.Windows.Input.MouseButton.XButton1)
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
            else
            {
                //cancelled dialog
            }

            //GC.Collect();
        }

#region FlyoutCommands
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

            ChangeTheme(Properties.Settings.Default.ThemeAccent);
            //ChangeAccent();//not needed since we calling ChangeTheme in there
        }

        private void OnThemeSwitch()
        {
            Properties.Settings.Default.DarkTheme = _darkModeToggle;
            ChangeTheme(Properties.Settings.Default.ThemeAccent);
        }

        private void ChangeTheme(int themeIndex)
        {
            if (_darkModeToggle)
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[themeIndex]), ThemeManager.GetAppTheme("BaseDark"));
            }
            else
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ThemeAccents[themeIndex]), ThemeManager.GetAppTheme("BaseLight"));
            }

            Properties.Settings.Default.Save();
        }

        private void OnAccentChanged()
        {
            Properties.Settings.Default.ThemeAccent = _themeAccentDropIndex;
            ChangeTheme(Properties.Settings.Default.ThemeAccent);//since theme also is rooted with accent
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

        private void OnOpenFileLocation(object sender, RoutedEventArgs e)
        {
            ExploreFile();
        }

        public void ExploreFile()
        {
            try
            {
                if (File.Exists(ImagesFound[ImageIndex]))
                {
                    //Clean up file path so it can be navigated OK
                    Process.Start("explorer.exe", string.Format("/select,\"{0}\"", Path.GetFullPath(ImagesFound[ImageIndex])));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (IsDeletingFile) return;

            DeleteToRecycleAsync(ImagesFound[ImageIndex]);
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
                        Title = "Deleting " + Path.GetFileName(path) + "...";
                    });

                    if (FileSystem.FileExists(path))
                    {
                        FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
                        //remove removed item from list

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ImagesFound.RemoveAt(ImageIndex);
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
            });
        }

        private void OnDownsizeSwitch()
        {
            Properties.Settings.Default.DownsizeImage = downsizeImageToggle;

            if (ImagesFound.Count > 0)
                ImageSource = LoadImage(new Uri(ImagesFound[ImageIndex], UriKind.Absolute));

            Properties.Settings.Default.Save();
        }

        private void OnZoomSensitivitySlider()
        {
            Properties.Settings.Default.ZoomSensitivity = zoomSensitivity;

            Properties.Settings.Default.Save();
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
