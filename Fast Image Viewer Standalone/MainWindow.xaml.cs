﻿using FIVStandard.Backend;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FIVStandard
{
    public partial class MainWindow : MetroWindow
    {
        public int imageIndex { get; set; } = 0;

        private bool isPaused = false;

        public int imgWidth { get; set; } = 0;
        public int imgHeight { get; set; } = 0;

        //private string startupPath;//program startup path

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        private readonly string[] themeAccents = new string[] { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };

        public static bool isDeletingFile { get; private set; } = false;

        public static double zoomSensitivity = 0.2;

        private readonly FileLoader fileLoader;

        public MainWindow()
        {
            InitializeComponent();

            fileLoader = new FileLoader(this);

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
                //string path = "D:\\Google Drive\\temp\\qmrns28.gif";

                //OpenNewFile(path);
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
            this.Title = $"[{imageIndex + 1}/{fileLoader.imagesFound.Count}] {Path.GetFileName(fileLoader.imagesFound[imageIndex])}";
        }

        /// <summary>
        /// Clear all saved paths and clean media view and finally cleanup memory
        /// </summary>
        private void ClearAllMedia()
        {
            fileLoader.imagesFound.Clear();
            MediaView.Source = null;
            PictureView.Source = null;
            ImageInfoText.Text = string.Empty;
            this.Title = "FIV";

            //GC.Collect();
        }

        public void OnClipOpened(object sender, RoutedEventArgs e)
        {
            if (fileLoader.imagesFound.Count == 0) return;

            using (var imageStream = File.OpenRead(fileLoader.imagesFound[imageIndex]))
            {
                var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                imgWidth = decoder.Frames[0].PixelWidth;
                imgHeight = decoder.Frames[0].PixelHeight;

                ImageInfoText.Text = $"{imgWidth}x{imgHeight}";
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
            if (fileLoader.imagesFound.Count == 0)//no more images in the folder - go back to default null
            {
                ClearAllMedia();
                return;
            }

            imageIndex += jump;
            if (imageIndex < 0) imageIndex = fileLoader.imagesFound.Count - 1;
            if (imageIndex >= fileLoader.imagesFound.Count) imageIndex = 0;

            if (!FileSystem.FileExists(fileLoader.imagesFound[imageIndex]))//keep moving onward until we find an existing file
            {
                //refresh the file lists in the directory
                //GetDirectoryFiles(Path.GetDirectoryName(imagesFound[imageIndex]));
                //FindIndexInFiles(imagesFound[imageIndex]);

                //remove nonexistent file from list - if there are more than 1
                if (fileLoader.imagesFound.Count > 1)
                {
                    fileLoader.imagesFound.RemoveAt(imageIndex);
                    SetTitleInformation();
                }

                ChangeImage(jump);

                return;
            }

            fileLoader.NewUri(fileLoader.imagesFound[imageIndex]);

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

            if (fileLoader.isAnimated)
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

        public void ImageChanged()
        {
            if (fileLoader.isAnimated)
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

            if (e.Key == System.Windows.Input.Key.Space)
            {
                TogglePause();
            }

            if (e.Key == System.Windows.Input.Key.Delete && fileLoader.imagesFound.Count > 0)
            {
                DeleteToRecycle(fileLoader.imagesFound[imageIndex]);
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

        private void OnOpenBrowseImage(object sender, RoutedEventArgs e)
        {
            if (isDeletingFile) return;

            Nullable<bool> result = fileLoader.openFileDialog.ShowDialog();
            if (result == true)
            {
                fileLoader.OpenNewFile(fileLoader.openFileDialog.FileName);
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
            DarkModeToggle.IsChecked = Properties.Settings.Default.DarkTheme;
            //Accent
            ThemeAccentDrop.ItemsSource = themeAccents;//init for theme list
            ThemeAccentDrop.SelectedIndex = Properties.Settings.Default.ThemeAccent;
            //Image Stretch
            StretchImageToggle.IsChecked = Properties.Settings.Default.ImageStretched;
            //Downsize Images
            DownsizeImageToggle.IsChecked = Properties.Settings.Default.DownsizeImage;
            //Zoom Sensitivity
            ZoomSensitivitySlider.Value = Properties.Settings.Default.ZoomSensitivity;

            ChangeStretch();

            ChangeTheme(Properties.Settings.Default.ThemeAccent);
            //ChangeAccent();//not needed since we calling ChangeTheme in there
        }

        private void OnThemeSwitch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DarkTheme = !Properties.Settings.Default.DarkTheme;
            ChangeTheme(Properties.Settings.Default.ThemeAccent);
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

        private void OnAccentChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.ThemeAccent = ThemeAccentDrop.SelectedIndex;
            ChangeAccent();
        }

        private void ChangeAccent()
        {
            ChangeTheme(Properties.Settings.Default.ThemeAccent);//since theme also is rooted with accent
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

        private void OnOpenFileLocation(object sender, RoutedEventArgs e)
        {
            ExploreFile();
        }

        public void ExploreFile()
        {
            try
            {
                if (File.Exists(fileLoader.imagesFound[imageIndex]))
                {
                    //Clean up file path so it can be navigated OK
                    Process.Start("explorer.exe", string.Format("/select,\"{0}\"", Path.GetFullPath(fileLoader.imagesFound[imageIndex])));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (isDeletingFile) return;

            DeleteToRecycle(fileLoader.imagesFound[imageIndex]);
        }

        private async void DeleteToRecycle(string path)
        {
            await Task.Run(() =>
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
                            fileLoader.imagesFound.RemoveAt(imageIndex);
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
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\nIndex: " + imageIndex);
                }
            }
            );
        }

        private void OnDownsizeSwitch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DownsizeImage = !Properties.Settings.Default.DownsizeImage;
            ChangeDownsize();
        }

        private void ChangeDownsize()
        {
            PictureView.Source = fileLoader.LoadImage(new Uri(fileLoader.imagesFound[imageIndex], UriKind.Absolute));

            Properties.Settings.Default.Save();
        }

        private void OnZoomSensitivitySlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Properties.Settings.Default.ZoomSensitivity = ZoomSensitivitySlider.Value;
            ChangeZoomSensitivity();
        }

        private void ChangeZoomSensitivity()
        {
            zoomSensitivity = Properties.Settings.Default.ZoomSensitivity;
            ZoomSensitivityText.Text = zoomSensitivity.ToString();

            Properties.Settings.Default.Save();
        }
    }
}
