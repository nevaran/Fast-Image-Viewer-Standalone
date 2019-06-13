﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FIVStandard.Backend
{
    class FileLoader
    {
<<<<<<< HEAD:Fast Image Viewer Standalone/Models/FileLoader.cs
        private readonly MainWindow that;
=======
        private readonly MainView MainVM;
>>>>>>> parent of 003606e... wth is going on:Fast Image Viewer Standalone/Models/FileLoaderModel.cs

        public bool IsAnimated { get; private set; } = false;

        public List<string> ImagesFound { get; set; } = new List<string>();

        private readonly string[] filters = new string[] { ".jpg", ".jpeg", ".png", ".gif"/*, ".tiff"*/, ".bmp"/*, ".svg"*/, ".ico"/*, ".mp4", ".avi" */};//TODO: doesnt work: tiff svg
        public OpenFileDialog DoOpenFileDialog { get; set; }  = new OpenFileDialog() { Filter = "Images (*.JPG, *.JPEG, *.PNG, *.GIF, *.BMP, *ICO)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.ICO"/* + "|All files (*.*)|*.*" */};

<<<<<<< HEAD:Fast Image Viewer Standalone/Models/FileLoader.cs
        public FileLoader(MainWindow mainWindow)
        {
            that = mainWindow;
=======
        public FileLoaderModel(MainView _mainView)
        {
            MainVM = _mainView;
>>>>>>> parent of 003606e... wth is going on:Fast Image Viewer Standalone/Models/FileLoaderModel.cs
        }

        public void OpenNewFile(string path)
        {
            if (MainWindow.IsDeletingFile) return;

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
                    that.ImageIndex = i;
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
                IsAnimated = true;
            }
            else
                IsAnimated = false;

            Uri uri = new Uri(path, UriKind.Absolute);

            that.MediaView?.Close();
            that.MediaView.Source = null;

            that.PictureView.Source = null;

            if (IsAnimated)
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

<<<<<<< HEAD:Fast Image Viewer Standalone/Models/FileLoader.cs
<<<<<<< HEAD:Fast Image Viewer Standalone/Models/FileLoaderModel.cs
                MainVM.ImageSource = LoadImage(uri, MainVM.DownsizeImageToggle);
=======
                that.PictureView.Source = LoadImage(uri);
>>>>>>> parent of 1af23f8... mvvm stuff:Fast Image Viewer Standalone/Models/FileLoader.cs
=======
                MainVM.ImageSource = LoadImage(uri);
>>>>>>> parent of 2cbf28d... semi-working mvvm, downsize broken:Fast Image Viewer Standalone/Models/FileLoaderModel.cs
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
<<<<<<< HEAD:Fast Image Viewer Standalone/Models/FileLoaderModel.cs
                if (MainVM.ImgWidth > MainVM.BorderImageWidth)
                    imgTemp.DecodePixelWidth = (int)MainVM.BorderImageWidth;
<<<<<<< HEAD:Fast Image Viewer Standalone/Models/FileLoader.cs
                else if (MainVM.ImgHeight > MainVM.BorderImageHeight)
                    imgTemp.DecodePixelHeight = (int)MainVM.BorderImageHeight;
=======
                if (that.ImgWidth > that.borderImg.ActualWidth)
                    imgTemp.DecodePixelWidth = (int)that.borderImg.ActualWidth;
                else if (that.ImgHeight > that.borderImg.ActualHeight)
                    imgTemp.DecodePixelHeight = (int)that.borderImg.ActualHeight;
>>>>>>> parent of 1af23f8... mvvm stuff:Fast Image Viewer Standalone/Models/FileLoader.cs
=======
                else if (MainVM.ImgHeight > MainVM.BorderImageWidth)
                    imgTemp.DecodePixelHeight = (int)MainVM.BorderImageWidth;
>>>>>>> parent of 2cbf28d... semi-working mvvm, downsize broken:Fast Image Viewer Standalone/Models/FileLoaderModel.cs
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
