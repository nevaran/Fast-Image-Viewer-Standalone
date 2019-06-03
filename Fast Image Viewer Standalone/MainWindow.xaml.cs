using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FIVStandard
{
    public partial class MainWindow : MetroWindow
    {
        int imageIndex = 0;
        List<string> imagesFound = new List<string>();
        readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif"/*, ".tiff"*/, ".bmp"/*, ".svg"*/, ".ico"/*, ".mp4", ".avi" */};//TODO: doesnt work: tiff svg

        bool isAnimated = false;
        bool isPaused = false;

        int imgWidth = 0;
        int imgHeight = 0;

        //private string startupPath;//program startup path

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        readonly string[] themeAccents = new string[] { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };
        OpenFileDialog ofd = new OpenFileDialog() { Filter = "Images (*.JPG, *.JPEG, *.PNG, *.GIF, *.BMP, *ICO)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.ICO"/* + "|All files (*.*)|*.*" */};

        public MainWindow()
        {
            InitializeComponent();

            LoadAllSettings();

            //AppWindow = this;//used for debugging ZoomBorder
        }

        private void OnAppLoaded(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 0)//get startup path
            {
                //startupPath = Path.GetDirectoryName(args[0]);

#if DEBUG
                string path = "D:\\Google Drive\\temp\\qmrns28.gif";

                OpenNewFile(path);
#endif
            }

            if (args.Length > 1)
            {
                OpenNewFile(args[1]);
            }
        }

        public void OpenNewFile(string path)
        {
            if (isDeletingFile) return;

            GetDirectoryFiles(Path.GetDirectoryName(path));

            FindIndexInFiles(path);
            SetTitleInformation();

            NewUri(path);
        }

        private void GetDirectoryFiles(string searchFolder)
        {
            imagesFound.Clear();
            List<string> filesFound = new List<string>();

            //filesFound.AddRange(Directory.GetFiles(searchFolder, "*.*", SearchOption.TopDirectoryOnly));
            filesFound.AddRange(Directory.EnumerateFiles(searchFolder).OrderBy(filename => filename));
            //filesFound.OrderBy(p => p.Substring(0)).ToList();//probably doesnt work

            int c = filesFound.Count;
            for (int i = 0; i < c; i++)
            {
                if (filters.Any(Path.GetExtension(filesFound[i]).Contains))
                {
                    imagesFound.Add(filesFound[i]);
                }
            }
        }

        private void FindIndexInFiles(string openedPathFile)
        {
            int L = imagesFound.Count;
            for (int i = 0; i < L; i++)
            {
                if(openedPathFile == imagesFound[i])
                {
                    imageIndex = i;
                    //MessageBox.Show(imagesFound.Count + " | " + imageIndex);//DEBUG
                    break;
                }
            }
        }

        private void OnClipOpened(object sender, RoutedEventArgs e)
        {
            if (imagesFound.Count == 0) return;

            using (var imageStream = File.OpenRead(imagesFound[imageIndex]))
            {
                var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                imgWidth = decoder.Frames[0].PixelWidth;
                imgHeight = decoder.Frames[0].PixelHeight;

                ImageInfoText.Text = $"{imgWidth}x{imgHeight}";
            }

            /*if (MediaView.NaturalDuration.HasTimeSpan)
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
            if (imagesFound.Count == 0)//no more images in the folder - go back to default null
            {
                ClearAllMedia();
                return;
            }

            imageIndex += jump;
            if (imageIndex < 0) imageIndex = imagesFound.Count - 1;
            if (imageIndex >= imagesFound.Count) imageIndex = 0;

            if (!FileSystem.FileExists(imagesFound[imageIndex]))//keep moving onward until we find an existing file
            {
                //refresh the file lists in the directory
                //GetDirectoryFiles(Path.GetDirectoryName(imagesFound[imageIndex]));
                //FindIndexInFiles(imagesFound[imageIndex]);

                //remove nonexistent file from list - if there are more than 1
                if (imagesFound.Count > 1)
                {
                    imagesFound.RemoveAt(imageIndex);
                    SetTitleInformation();
                }

                ChangeImage(jump);

                return;
            }

            NewUri(imagesFound[imageIndex]);

            SetTitleInformation();
        }

        /// <summary>
        /// Clear all saved paths and clean media view and finally cleanup memory
        /// </summary>
        private void ClearAllMedia()
        {
            imagesFound.Clear();
            MediaView.Source = null;
            PictureView.Source = null;
            ImageInfoText.Text = string.Empty;
            this.Title = "FIV";

            GC.Collect();
        }

        private void SetTitleInformation()
        {
            this.Title = $"[{imageIndex + 1}/{imagesFound.Count}] {Path.GetFileName(imagesFound[imageIndex])}";
        }

        private void TogglePause()
        {
            /*controller = ImageBehavior.GetAnimationController(MainImage);

            if (!isAnimated) return;

            if (controller.IsPaused)
                controller.Play();
            else
                controller.Pause();*/

            if (isAnimated)
            {
                if (isPaused)
                {
                    MediaView.Play();
                    isPaused = false;
                }
                else
                {
                    MediaView.Pause();
                    isPaused = true;
                }
            }
        }

        private void OnImageChanged()
        {
            if (isAnimated)
            {
                isPaused = false;

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
                isAnimated = true;
            }
            else
                isAnimated = false;

            Uri uri = new Uri(path, UriKind.Absolute);

            MediaView?.Close();
            MediaView.Source = null;

            PictureView.Source = null;

            if (isAnimated)
            {
                borderImg.Visibility = Visibility.Hidden;
                border.Visibility = Visibility.Visible;

                MediaView.Source = uri;
            }
            else
            {
                borderImg.Visibility = Visibility.Visible;
                border.Visibility = Visibility.Hidden;

                OnClipOpened(null, null);

                PictureView.Source = LoadImage(uri);
                //UpdateImage(path);
            }
            
            OnImageChanged();

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
                if(imgWidth > borderImg.ActualWidth)
                    imgTemp.DecodePixelWidth = (int)borderImg.ActualWidth;
                else if(imgHeight > borderImg.ActualHeight)
                    imgTemp.DecodePixelHeight = (int)borderImg.ActualHeight;
            }
            imgTemp.EndInit();
            imgTemp.Freeze();

            return imgTemp;
        }

        /*Stream mediaStream;

        void DisposeMediaStream()
        {
            if (mediaStream != null)
            {
                mediaStream.Close();
                mediaStream.Dispose();
                mediaStream = null;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            }
        }

        void UpdateImage(string path)
        {
            DisposeMediaStream();

            var bitmap = new BitmapImage();
            mediaStream = new FileStream(path, FileMode.Open);

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.None;
            bitmap.StreamSource = mediaStream;
            bitmap.EndInit();

            bitmap.Freeze();
            PictureView.Source = bitmap;
        }*/

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (isDeletingFile) return;

            if(e.Key == System.Windows.Input.Key.Right)
            {
                ChangeImage(1);//go forward
            }
            if(e.Key == System.Windows.Input.Key.Left)
            {
                ChangeImage(-1);//go back
            }

            /*if (e.Key == System.Windows.Input.Key.Up)
            {
                ChangeFrame(1);//go forward
            }
            if (e.Key == System.Windows.Input.Key.Down)
            {
                ChangeFrame(-1);//go back
            }*/

            if (e.Key == System.Windows.Input.Key.Space)
            {
                TogglePause();
            }

            if (e.Key == System.Windows.Input.Key.Delete && imagesFound.Count > 0)
            {
                DeleteToRecycle(imagesFound[imageIndex]);
            }

            if (e.Key == System.Windows.Input.Key.F)
            {
                StretchImageToggle.IsChecked = !StretchImageToggle.IsChecked;
            }

            if (e.Key == System.Windows.Input.Key.E)
            {
                ExploreFile();
            }
        }

        private void OnClick_Next(object sender, RoutedEventArgs e)
        {
            if (isDeletingFile) return;

            ChangeImage(1);//go forward
        }

        private void OnClick_Prev(object sender, RoutedEventArgs e)
        {
            if (isDeletingFile) return;

            ChangeImage(-1);//go back
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isDeletingFile) return;

            if(e.ChangedButton == System.Windows.Input.MouseButton.XButton1)
            {
                ChangeImage(-1);//go back
            }
            if (e.ChangedButton == System.Windows.Input.MouseButton.XButton2)
            {
                ChangeImage(1);//go forward
            }
        }

        private void OpenBrowseImage(object sender, RoutedEventArgs e)
        {
            if (isDeletingFile) return;

            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            {
                OpenNewFile(ofd.FileName);
            }
            else
            {
                //cancelled dialog
            }

            //GC.Collect();
        }

        static bool isDeletingFile = false;
        private void DeleteToRecycle(string path)
        {
            Task.Run(() =>
            {
                try
                {
                    isDeletingFile = true;

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
                            imagesFound.RemoveAt(imageIndex);
                            ChangeImage(-1);//go back to a previous file after deletion
                            //SetTitleInformation();
                        });
                    }
                    else
                    {
                        MessageBox.Show("File not found: " + path);
                    }

                    isDeletingFile = false;
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message + "\nIndex: " + imageIndex);
                }
            }
            );
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            HelpFlyout.IsOpen = false;
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
        }

        private void OnDonateClick(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=6ZXTCHB3JXL4Q&source=url");
            Process.Start(sInfo);
        }

        private void LoadAllSettings()
        {
            //Theme
            DarkModeToggle.IsChecked = Properties.Settings.Default.DarkTheme;
            //Accent
            ThemeAccentDrop.ItemsSource = themeAccents;//init for theme list
            ThemeAccentDrop.SelectedIndex = Properties.Settings.Default.ThemeAccent;
            //Image Stretch
            StretchImageToggle.IsChecked = Properties.Settings.Default.ImageStretched;
            //Downsize Images
            DownsizeImageToggle.IsChecked = Properties.Settings.Default.DownsizeImage;

            ChangeStretch();

            ChangeTheme(Properties.Settings.Default.ThemeAccent);
            //ChangeAccent();//not needed since we calling ChangeTheme in there
        }

        private void ChangeTheme(int themeIndex)
        {
            if (Properties.Settings.Default.DarkTheme)
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(themeAccents[themeIndex]), ThemeManager.GetAppTheme("BaseDark"));
            }
            else
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(themeAccents[themeIndex]), ThemeManager.GetAppTheme("BaseLight"));
            }

            Properties.Settings.Default.Save();
        }

        private void ChangeAccent()
        {
            ChangeTheme(Properties.Settings.Default.ThemeAccent);//since theme also is rooted with accent
        }

        private void ChangeStretch()
        {
            if (Properties.Settings.Default.ImageStretched)
            {
                MediaView.StretchDirection = System.Windows.Controls.StretchDirection.Both;
                PictureView.StretchDirection = System.Windows.Controls.StretchDirection.Both;
            }
            else
            {
                MediaView.StretchDirection = System.Windows.Controls.StretchDirection.DownOnly;
                PictureView.StretchDirection = System.Windows.Controls.StretchDirection.DownOnly;
            }

            Properties.Settings.Default.Save();
        }

        private void OnThemeSwitch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DarkTheme = !Properties.Settings.Default.DarkTheme;
            ChangeTheme(Properties.Settings.Default.ThemeAccent);
        }

        private void OnAccentChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.ThemeAccent = ThemeAccentDrop.SelectedIndex;
            ChangeAccent();
        }

        private void OnAccentClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ThemeAccent++;

            if (Properties.Settings.Default.ThemeAccent > themeAccents.Length - 1)
                Properties.Settings.Default.ThemeAccent = 0;

            ThemeAccentDrop.SelectedIndex = Properties.Settings.Default.ThemeAccent;

            //ChangeAccent();//called in OnAccentChanged
        }

        private void OnStretchSwitch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ImageStretched = !Properties.Settings.Default.ImageStretched;
            ChangeStretch();
        }

        public void ExploreFile()
        {
            try
            {
                if (File.Exists(imagesFound[imageIndex]))
                {
                    //Clean up file path so it can be navigated OK
                    Process.Start("explorer.exe", string.Format("/select,\"{0}\"", Path.GetFullPath(imagesFound[imageIndex])));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ClickCloseSettings(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SettingsFlyout.IsOpen = false;
            HelpFlyout.IsOpen = false;
        }

        private void OnOpenFileLocation(object sender, RoutedEventArgs e)
        {
            ExploreFile();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (isDeletingFile) return;

            DeleteToRecycle(imagesFound[imageIndex]);
        }

        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = false;
            HelpFlyout.IsOpen = !HelpFlyout.IsOpen;
        }

        private void OnDownsizeSwitch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DownsizeImage = !Properties.Settings.Default.DownsizeImage;
            ChangeDownsize();
        }

        private void ChangeDownsize()
        {
            PictureView.Source = LoadImage(new Uri(imagesFound[imageIndex], UriKind.Absolute));
            //UpdateImage(imagesFound[imageIndex]);

            Properties.Settings.Default.Save();
        }
    }
}
