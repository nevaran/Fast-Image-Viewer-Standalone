using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Runtime.InteropServices;
//using System.Security.AccessControl;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace FIVStandard
{
    public partial class MainWindow : Window
    {
        //[DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        BitmapImage bm;

        int imageIndex = 0;
        List<string> imagesFound = new List<string>();
        readonly string[] filters = new string[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg", "ico" };

        ImageAnimationController controller;
        bool isAnimated = false;
        bool isPaused = false;

        private string startupPath;//program startup path

        //public static MainWindow AppWindow;//used for debugging ZoomBorder

        public MainWindow()
        {
            InitializeComponent();

            //AppWindow = this;//used for debugging ZoomBorder

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 0)//get startup path
            {
                startupPath = Path.GetDirectoryName(args[0]);

#if DEBUG
                string path = "D:\\Google Drive\\temp\\p1asN.gif";
                GetDirectoryFiles(Path.GetDirectoryName(path));

                FindIndexInFiles(path);
                SetTitleInformation();

                NewUri(path);
#endif
            }

            //Associate(startupPath);

            if (args.Length > 1)
            {
                GetDirectoryFiles(Path.GetDirectoryName(args[1]));

                FindIndexInFiles(args[1]);
                SetTitleInformation();

                NewUri(args[1]);
            }
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
                    break;
                    //MessageBox.Show(imagesFound.Count + " | " + imageIndex);//DEBUG
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
            if (imagesFound.Count < 2) return;//dont try change if only image found

            imageIndex += jump;
            if (imageIndex < 0) imageIndex = imagesFound.Count - 1;
            if (imageIndex >= imagesFound.Count) imageIndex = 0;

            NewUri(imagesFound[imageIndex]);

            SetTitleInformation();

            /*if (filters.Any(Path.GetExtension(imagesFound[imageIndex]).Contains))//if file is with one of the filter exceptions
            {
                NewUri(imagesFound[imageIndex]);
            }
            else
                ChangeImage(jump);*/
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

            if (Path.GetExtension(path) == ".gif")
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
        }

        private void OnClick_Next(object sender, RoutedEventArgs e)
        {
            ChangeImage(1);//go forward
        }

        private void OnClick_Prev(object sender, RoutedEventArgs e)
        {
            ChangeImage(-1);//go back
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.ChangedButton == System.Windows.Input.MouseButton.XButton1)
            {
                ChangeImage(-1);//go back
            }
            if (e.ChangedButton == System.Windows.Input.MouseButton.XButton2)
            {
                ChangeImage(1);//go forward
            }
        }

        OpenFileDialog ofd = new OpenFileDialog();
        private void OpenBrowseImage(object sender, RoutedEventArgs e)
        {
            ofd.ShowDialog();

            GC.Collect();
        }

        /*public static void Associate(string startupPath)
        {
            RegistryKey FileRegJpg  = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.jpg");
            RegistryKey FileRegJpeg = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.jpeg");
            RegistryKey FileRegPng  = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.png");
            RegistryKey FileRegGif  = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.gif");
            RegistryKey FileRegTiff = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.tiff");
            RegistryKey FileRegBmp  = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.bmp");
            RegistryKey FileRegSvg  = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.svg");
            RegistryKey FileRegIco  = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.ico");

            RegistryKey AppReg = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Applications\\Fast Image Viewer Standalone.exe");//TODO: Check if app name is correct

            //TODO: check if this is required
            string user = Environment.UserDomainName + "\\" + Environment.UserName;
            RegistrySecurity rs = new RegistrySecurity();
            rs.AddAccessRule(new RegistryAccessRule(user, RegistryRights.WriteKey | RegistryRights.ChangePermissions, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));

            RegistryKey AppAssocJpg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.jpg", RegistryKeyPermissionCheck.ReadWriteSubTree, rs);
            RegistryKey AppAssocJpeg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.jpeg", RegistryKeyPermissionCheck.ReadWriteSubTree, rs);
            RegistryKey AppAssocPng  = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.png", RegistryKeyPermissionCheck.ReadWriteSubTree, rs);
            RegistryKey AppAssocGif  = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.gif", RegistryKeyPermissionCheck.ReadWriteSubTree, rs);
            RegistryKey AppAssocTiff = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.tiff", RegistryKeyPermissionCheck.ReadWriteSubTree, rs);
            RegistryKey AppAssocBmp  = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.bmp", RegistryKeyPermissionCheck.ReadWriteSubTree, rs);
            RegistryKey AppAssocSvg  = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.svg", RegistryKeyPermissionCheck.ReadWriteSubTree, rs);
            RegistryKey AppAssocIco  = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.ico", RegistryKeyPermissionCheck.ReadWriteSubTree, rs);

            //animated = anim.ico ; normal = def.ico
            FileRegJpg.CreateSubKey("DefaultIcon").SetValue("", startupPath + "\\def.ico");
            FileRegJpg.CreateSubKey("PerievedType").SetValue("", startupPath + "Image");
            FileRegJpeg.CreateSubKey("DefaultIcon").SetValue("", startupPath + "\\def.ico");
            FileRegJpeg.CreateSubKey("PerievedType").SetValue("", startupPath + "Image");
            FileRegPng.CreateSubKey("DefaultIcon").SetValue("", startupPath + "\\def.ico");
            FileRegPng.CreateSubKey("PerievedType").SetValue("", startupPath + "Image");
            FileRegGif.CreateSubKey("DefaultIcon").SetValue("", startupPath + "\\anim.ico");
            FileRegGif.CreateSubKey("PerievedType").SetValue("", startupPath + "Animated Image");
            FileRegTiff.CreateSubKey("DefaultIcon").SetValue("", startupPath + "\\def.ico");
            FileRegTiff.CreateSubKey("PerievedType").SetValue("", startupPath + "Image");
            FileRegBmp.CreateSubKey("DefaultIcon").SetValue("", startupPath + "\\def.ico");
            FileRegBmp.CreateSubKey("PerievedType").SetValue("", startupPath + "Image");
            FileRegSvg.CreateSubKey("DefaultIcon").SetValue("", startupPath + "\\def.ico");
            FileRegSvg.CreateSubKey("PerievedType").SetValue("", startupPath + "Image");
            FileRegIco.CreateSubKey("DefaultIcon").SetValue("", startupPath + "\\def.ico");
            FileRegIco.CreateSubKey("PerievedType").SetValue("", startupPath + "Image");

            AppReg.CreateSubKey("shell\\open\\command").SetValue("", startupPath + "\" %1");//open context
            //AppReg.CreateSubKey("shell\\edit\\command").SetValue("", startupPath + "\" %1");//edit context
            AppReg.CreateSubKey("DefaultIcon").SetValue("", startupPath + "\\def.ico");

            //TODO: fix access denied on UserChoice
            AppAssocJpg.CreateSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, rs).SetValue("ProgId", "Applications\\Fast Image Viewer Standalone.exe");
            AppAssocJpeg.CreateSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, rs).SetValue("ProgId", "Applications\\Fast Image Viewer Standalone.exe");
            AppAssocPng.CreateSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, rs).SetValue("ProgId", "Applications\\Fast Image Viewer Standalone.exe");
            AppAssocGif.CreateSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, rs).SetValue("ProgId", "Applications\\Fast Image Viewer Standalone.exe");
            AppAssocTiff.CreateSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, rs).SetValue("ProgId", "Applications\\Fast Image Viewer Standalone.exe");
            AppAssocBmp.CreateSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, rs).SetValue("ProgId", "Applications\\Fast Image Viewer Standalone.exe");
            AppAssocSvg.CreateSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, rs).SetValue("ProgId", "Applications\\Fast Image Viewer Standalone.exe");
            AppAssocIco.CreateSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, rs).SetValue("ProgId", "Applications\\Fast Image Viewer Standalone.exe");

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }*/
    }
}
