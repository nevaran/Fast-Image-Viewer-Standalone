using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FIVStandard.Backend
{
    public class FileLoaderModel
    {
        private readonly MainView MainVM;

        public bool IsAnimated { get; private set; } = false;

        public List<string> ImagesFound { get; set; } = new List<string>();

        private readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif"/*, ".tiff"*/, ".bmp"/*, ".svg"*/, ".ico"/*, ".mp4", ".avi" */};//TODO: doesnt work: tiff svg
        public OpenFileDialog DoOpenFileDialog { get; set; }  = new OpenFileDialog() { Filter = "Images (*.JPG, *.JPEG, *.PNG, *.GIF, *.BMP, *ICO)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.ICO"/* + "|All files (*.*)|*.*" */};

        public FileLoaderModel(MainView _mainView)
        {
            MainVM = _mainView;
        }

        public void OpenNewFile(string path)
        {
            if (MainVM.IsDeletingFile) return;

            GetDirectoryFiles(Path.GetDirectoryName(path));

            FindIndexInFiles(path);

            NewUri(path);
        }

        private void GetDirectoryFiles(string searchFolder)
        {
            ImagesFound.Clear();
            List<string> filesFound = new List<string>();//all files found in the directory

            //filesFound.AddRange(Directory.GetFiles(searchFolder, "*.*", SearchOption.TopDirectoryOnly));
            //filesFound.AddRange(Directory.EnumerateFiles(searchFolder).OrderBy(filename => filename));
            //filesFound.OrderBy(p => p.Substring(0)).ToList();//probably doesnt work

            filesFound.AddRange(Directory.EnumerateFiles(searchFolder));

            int c = filesFound.Count;
            for (int i = 0; i < c; i++)
            {
                if (filters.Any(Path.GetExtension(filesFound[i]).Contains))//add files only with set file type in filters
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
                if (openedPathFile == ImagesFound[i])
                {
                    MainVM.ImageIndex = i;
                    //MessageBox.Show(imagesFound.Count + " | " + imageIndex);//DEBUG
                    break;
                }
            }
        }

        public void NewUri(string path)
        {
            string pathext = Path.GetExtension(path);
            if (pathext == ".gif"/* || pathext == ".mp4" || pathext == ".avi"*/)
                IsAnimated = true;
            else
                IsAnimated = false;

            Uri uri = new Uri(path, UriKind.Absolute);

            //MainVM.MediaSource?.Close();
            MainVM.MediaSource = null;

            MainVM.ImageSource = null;

            if (IsAnimated)
            {
                MainVM.BorderImgVisible = Visibility.Hidden;
                MainVM.BorderMediaVisible = Visibility.Visible;

                MainVM.MediaSource = uri;
            }
            else
            {
                MainVM.BorderImgVisible = Visibility.Visible;
                MainVM.BorderMediaVisible = Visibility.Hidden;

                MainVM.OnClipOpened(null, null);

                MainVM.ImageSource = LoadImage(uri, MainVM.DownsizeImageToggle);
            }

            MainVM.ImageChanged();

            //GC.Collect();
        }

        public BitmapImage LoadImage(Uri uri, bool downsizedImage)
        {
            BitmapImage imgTemp = new BitmapImage();
            imgTemp.BeginInit();
            imgTemp.CacheOption = BitmapCacheOption.OnLoad;
            imgTemp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            imgTemp.UriSource = uri;
            if (downsizedImage)
            {
                if (MainVM.ImgWidth > MainVM.BorderImageWidth)
                    imgTemp.DecodePixelWidth = (int)MainVM.BorderImageWidth;
                else if (MainVM.ImgHeight > MainVM.BorderImageHeight)
                    imgTemp.DecodePixelHeight = (int)MainVM.BorderImageHeight;
            }
            imgTemp.EndInit();
            imgTemp.Freeze();

            return imgTemp;
        }
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
}
