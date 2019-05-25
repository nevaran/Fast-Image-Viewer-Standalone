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

namespace FIVStandard
{
    public partial class MainWindow : MetroWindow
    {
        //BitmapImage bm;

        public int imageIndex = 0;
        List<string> imagesFound = new List<string>();
        readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif"/*, ".tiff"*/, ".bmp"/*, ".svg"*/, ".ico", ".mp4" };//TODO: doesnt work: tiff svg ico

        bool isAnimated = false;
        bool isPaused = false;

        //private string startupPath;//program startup path

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        public MainWindow()
        {
            InitializeComponent();

            //AppWindow = this;//used for debugging ZoomBorder

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 0)//get startup path
            {
                //startupPath = Path.GetDirectoryName(args[0]);

#if DEBUG
                string path = "D:\\Google Drive\\temp\\qmrns28.gif";
                GetDirectoryFiles(Path.GetDirectoryName(path));

                FindIndexInFiles(path);
                SetTitleInformation();

                NewUri(path);
#endif
            }

            if (args.Length > 1)
            {
                GetDirectoryFiles(Path.GetDirectoryName(args[1]));

                FindIndexInFiles(args[1]);
                SetTitleInformation();

                NewUri(args[1]);
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

        /*private void ChangeFrame(int jump)
        {
            if (!isAnimated || controller == null) return;

            if (!controller.IsPaused)
                controller.Pause();

            controller.GotoFrame(controller.CurrentFrame + jump);
        }*/

        private void OnImageChanged()
        {
            isPaused = false;

            MediaView.Play();

            border.Reset();
            //MainImage.Source = bm;
            //ImageBehavior.SetAnimatedSource(MainImage, bm);
            //ImageGrid.Margin = new Thickness(0, 0, 0, 60);
        }

        private void NewUri(string path)
        {
            MediaView?.Close();
            MediaView.Source = null;
            MediaView.Source = new Uri(path, UriKind.Absolute);

            if (Path.GetExtension(path) == ".gif" || Path.GetExtension(path) == ".mp4")
            {
                isAnimated = true;
            }
            else
                isAnimated = false;

            OnImageChanged();

            GC.Collect();
        }

        //bm = new BitmapImage(new Uri(path, UriKind.Absolute));

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

        //"Images (*.JPG, *.JPEG, *.PNG, *.GIF, *.TIFF, *.BMP, *SVG, *ICO, *.MP4)|*.JPG;*.JPEG;*.PNG;*.GIF;*.TIFF;*.BMP;*SVG;*ICO;*MP4"
        OpenFileDialog ofd = new OpenFileDialog() { Filter = "Images (*.JPG, *.JPEG, *.PNG, *.GIF, *.BMP, *ICO, *.MP4)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*ICO;*MP4"/* + "|All files (*.*)|*.*" */};
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

            GC.Collect();
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
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
        }

        private void OnDonateClick(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=CNWYKDLHJW9CW");
            Process.Start(sInfo);
        }
    }
}
