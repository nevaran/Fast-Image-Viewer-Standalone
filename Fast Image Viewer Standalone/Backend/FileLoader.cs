using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FIVStandard.Backend
{
    class FileLoader
    {
        MainWindow that;

        public bool isAnimated = false;

        public List<string> imagesFound = new List<string>();

        private readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif"/*, ".tiff"*/, ".bmp"/*, ".svg"*/, ".ico"/*, ".mp4", ".avi" */};//TODO: doesnt work: tiff svg
        public OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Images (*.JPG, *.JPEG, *.PNG, *.GIF, *.BMP, *ICO)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.ICO"/* + "|All files (*.*)|*.*" */};

        public FileLoader(MainWindow mainWindow)
        {
            that = mainWindow;
        }

        public void OpenNewFile(string path)
        {
            if (MainWindow.isDeletingFile) return;

            GetDirectoryFiles(Path.GetDirectoryName(path));

            FindIndexInFiles(path);

            NewUri(path);
        }

        private void GetDirectoryFiles(string searchFolder)
        {
            imagesFound.Clear();
            List<string> filesFound = new List<string>();//all files found in the directory

            //filesFound.AddRange(Directory.GetFiles(searchFolder, "*.*", SearchOption.TopDirectoryOnly));
            filesFound.AddRange(Directory.EnumerateFiles(searchFolder).OrderBy(filename => filename));
            //filesFound.OrderBy(p => p.Substring(0)).ToList();//probably doesnt work

            int c = filesFound.Count;
            for (int i = 0; i < c; i++)
            {
                if (filters.Any(Path.GetExtension(filesFound[i]).Contains))//add files only with set file type in filters
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
                if (openedPathFile == imagesFound[i])
                {
                    that.imageIndex = i;
                    //MessageBox.Show(imagesFound.Count + " | " + imageIndex);//DEBUG
                    break;
                }
            }
        }

        public void NewUri(string path)
        {
            string pathext = Path.GetExtension(path);
            if (pathext == ".gif"/* || pathext == ".mp4" || pathext == ".avi"*/)
            {
                isAnimated = true;
            }
            else
                isAnimated = false;

            Uri uri = new Uri(path, UriKind.Absolute);

            that.MediaView?.Close();
            that.MediaView.Source = null;

            that.PictureView.Source = null;

            if (isAnimated)
            {
                that.borderImg.Visibility = Visibility.Hidden;
                that.border.Visibility = Visibility.Visible;

                that.MediaView.Source = uri;
            }
            else
            {
                that.borderImg.Visibility = Visibility.Visible;
                that.border.Visibility = Visibility.Hidden;

                that.OnClipOpened(null, null);

                that.PictureView.Source = LoadImage(uri);
            }

            that.ImageChanged();

            //GC.Collect();
        }

        public BitmapImage LoadImage(Uri uri)
        {
            BitmapImage imgTemp = new BitmapImage();
            imgTemp.BeginInit();
            imgTemp.CacheOption = BitmapCacheOption.OnLoad;
            imgTemp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            imgTemp.UriSource = uri;
            if (Properties.Settings.Default.DownsizeImage)
            {
                if (that.imgWidth > that.borderImg.ActualWidth)
                    imgTemp.DecodePixelWidth = (int)that.borderImg.ActualWidth;
                else if (that.imgHeight > that.borderImg.ActualHeight)
                    imgTemp.DecodePixelHeight = (int)that.borderImg.ActualHeight;
            }
            imgTemp.EndInit();
            imgTemp.Freeze();

            return imgTemp;
        }
    }
}
