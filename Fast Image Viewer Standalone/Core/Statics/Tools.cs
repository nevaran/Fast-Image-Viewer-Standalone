using FIVStandard.Utils;
using FIVStandard.ViewModel;
using ImageMagick;
using Notifications.Wpf.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static FIVStandard.Core.SettingsStore;

#pragma warning disable CA1416 // Validate platform compatibility

namespace FIVStandard.Core
{
    internal static class Tools
    {
        public static Task<BitmapSource> LoadImage(string path, int imgWidth, int imgHeight, MainWindow mainWindow, CancellationToken ct)
        {
            if (!File.Exists(path) || ct.IsCancellationRequested)
                return Task.FromResult((BitmapSource)null);

            try
            {
                using MagickImage image = new(path);

                if (Settings.DownsizeImageToggle)
                {
                    Rect? nullableRect = null;

                    Application.Current.Dispatcher.Invoke(() => { nullableRect = WpfScreen.GetScreenFrom(Application.Current.MainWindow).ScreenBounds; });

                    Rect r = nullableRect.Value;
                    if (imgWidth > r.Width || imgHeight > r.Height)
                        image.Resize((int)(imgWidth * ScaleToBox(imgWidth, (int)r.Width, imgHeight, (int)r.Height)), 0);
                }
                image.AutoOrient();

                if (ct.IsCancellationRequested)
                    return Task.FromResult((BitmapSource)null);

                BitmapSource bms = image.ToBitmapSource();
                bms.Freeze();

                mainWindow.IsLoading = false;

                return Task.FromResult(bms);
            }
            catch(MagickErrorException e)
            {
                mainWindow.IsLoading = false;

                mainWindow.NotificationContent.Title = e.Message;
                mainWindow.NotificationContent.Message = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.ErrorLoadingImageCorrupt), Localization.TranslationSource.Instance.CurrentCulture);
                mainWindow.NotificationContent.Type = NotificationType.Error;
                _ = mainWindow.NotificationManager.ShowAsync(mainWindow.NotificationContent, token: ct);

                return Task.FromResult((BitmapSource)null);
            }
        }

        /// <summary>
        /// Load a single thumbnail image item data to the defined position (via ThumbnailItemData reference).
        /// </summary>
        /// <param name="fullPath"> Complete path to the file, including the name and extension</param>
        /// <param name="tid"> Information of the file, including a thumbnail</param>
        /// <param name="overrideThumbnail"> If true: replace the thumbnail even if there is already one generated</param>
        /// <returns></returns>
        public static void LoadSingleThumbnailData(string fullPath, ThumbnailItemData tid, bool overrideThumbnail = false)
        {
            if (Path.GetExtension(fullPath) == ".webm" || !File.Exists(fullPath)) return;

            if (overrideThumbnail || tid.ThumbnailImage is null)//load thumbnail only if its set for override or is empty
                LoadThumbnailData(fullPath, tid);
        }

        private static void LoadThumbnailData(string path, ThumbnailItemData tid)
        {
            try
            {
                using MagickImage image = new(path);
                image.AutoOrient();

                image.Thumbnail(Settings.ThumbnailRes, 0);

                tid.ThumbnailImage = image.ToBitmapSource();
                tid.ThumbnailImage.Freeze();

                GetImageInformation(path, tid);
            }
            catch
            {
                tid.ThumbnailImage = null;
            }
        }

        /// <summary>
        /// Gets a thumbnail-sized image (if not already set), actual width and actual height of the image and assigns it to the given ThumbnailItemData,
        /// and checks if the image is not animated(gif).
        /// </summary>
        public static void GetImageInformation(string path, ThumbnailItemData ImageItem)
        {
            if (ImageItem.ThumbnailImage is null)
            {
                _ = Task.Run(() => LoadSingleThumbnailData(path, ImageItem));
            }

            if (ImageItem.ImageWidth != 0)//we already have a set width in the item data
            {
                return;
            }

            System.Collections.Generic.IEnumerable<IMagickImageInfo> collection = MagickImageInfo.ReadCollection(path);
            if (collection.Count() > 1)//check if the image is not secretly a gif or other animated image magick.net supports detecting
            {
                ImageItem.FileType = FileMediaType.Gif;
            }

            IMagickImageInfo first = collection.First();

            ImageItem.ImageWidth = first.Width;
            ImageItem.ImageHeight = first.Height;

            /*magickInfo.Read(path);
            ImageItem.ImageWidth = magickInfo.Width;
            ImageItem.ImageHeight = magickInfo.Height;*/
        }

        /// <summary>
        /// Checks if the string has an extension of the given valid types
        /// </summary>
        public static bool IsOfType(string file, string[] extensions)
        {
            string ext = Path.GetExtension(file.ToLower());

            return extensions.Any(ext.Contains);
        }

        /// <summary>
        /// Returns a scale multiplier that can be used for resizing width and height of an image to the given required size while keeping aspect ratios
        /// </summary>
        /// <param name="w">Image width</param>
        /// <param name="sw">Target width</param>
        /// <param name="h">Image height</param>
        /// <param name="sh">Target height</param>
        private static double ScaleToBox(double w, double sw, double h, double sh)
        {
            double scaleWidth = sw / w;
            double scaleHeight = sh / h;

            double scale = Math.Min(scaleWidth, scaleHeight);

            return scale;
        }

        /// <summary>
        /// Is the file type is of the supported animated type
        /// </summary>
        public static FileMediaType IsAnimatedExtension(string ext)
        {
            return ext switch
            {
                ".gif" => FileMediaType.Gif,
                ".webm" => FileMediaType.Video,
                _ => FileMediaType.Image,
            };
        }

        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            BitmapSource bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                ConvertPixelFormat(bitmap.PixelFormat), null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            bitmap.Dispose();

            return bitmapSource;
        }

        /// <summary>
        /// Used in method BitmapToBitmapSource; Can be used as standalone
        /// </summary>
        private static PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat)
        {
            switch (sourceFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr32;
                case System.Drawing.Imaging.PixelFormat.Indexed:
                    return PixelFormats.Bgr101010;
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                    return PixelFormats.Bgr555;
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    return PixelFormats.Bgr565;
                case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                    return PixelFormats.Indexed1;
                case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    return PixelFormats.Indexed4;
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return PixelFormats.Indexed8;
                case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                    return PixelFormats.Bgr32;
                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    return PixelFormats.Gray16;
                case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                    return PixelFormats.Rgb48;
                case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
                    return PixelFormats.Prgba64;
                case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
                    return PixelFormats.Prgba64;//uncertain
                case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                    return PixelFormats.Bgr555;
                case System.Drawing.Imaging.PixelFormat.DontCare:
                    break;
                case System.Drawing.Imaging.PixelFormat.Max:
                    break;
                case System.Drawing.Imaging.PixelFormat.Gdi:
                    break;
                case System.Drawing.Imaging.PixelFormat.Alpha:
                    break;
                case System.Drawing.Imaging.PixelFormat.PAlpha:
                    break;
                case System.Drawing.Imaging.PixelFormat.Extended:
                    break;
                case System.Drawing.Imaging.PixelFormat.Canonical:
                    break;
            }
            return PixelFormats.Bgra32;//give a "default" format if we dont have the case set
        }

        public static string RnJesus()//meme thing
        {
            Random rnd = new();
            string[] rnj = [
                        @"owo",
                        @"uwu",
                        @"ゴ ゴ ゴ ゴ",
                        @"I am Lagnar",
                        @"Made by ねヴぁらん",
                        @"¯\_(ツ)_/¯",
            ];

            //10% chance to show meme
            return rnd.Next(0, 11) == 0 ? rnj[rnd.Next(0, rnj.Length)] : string.Empty;
        }

        public static string GetAfter(this string s, char c)//returns the striing after a selected char
        {
            return s[(s.IndexOf(c) + 1)..];
            //return s.Substring(s.IndexOf(c) + 1);
        }

        /// <summary>
        /// Opens windows explorer to the designated file.
        /// To avoid crashes or bugs, use `Path.GetFullPath(path)`
        /// </summary>
        public static void ExploreFile(string path)
        {
            if (File.Exists(path))
            {
                _ = Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
        }

        public static Visibility BoolToVisibility(bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Returns only the URL of a `DragEventHandle.Data.GetData(DataFormats.Html)`
        /// </summary>
        public static string GetUrlSourceImage(string str)
        {
            string firstString = "src=\"";
            string lastString = "\"";

            int startPos = str.IndexOf(firstString) + firstString.Length;
            string modifiedString = str[startPos..];
            int endPos = modifiedString.IndexOf(lastString);
            string finalString = modifiedString[..endPos];

            return finalString;
        }

        public static void OpenHyperlink(string url, NotificationContent content, NotificationManager manager)
        {
            try
            {
                ProcessStartInfo sInfo = new(url) { UseShellExecute = true };
                _ = Process.Start(sInfo);
            }
            catch (Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                {
                    content.Title = "Hyperlink Error";
                    content.Message = noBrowser.Message;
                    content.Type = NotificationType.Error;
                    manager.ShowAsync(content);
                }
            }
            catch (Exception other)
            {
                content.Title = "Hyperlink Error";
                content.Message = other.Message;
                content.Type = NotificationType.Error;
                manager.ShowAsync(content);
            }
        }

        /*public static BitmapImage WriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }*/

        /*public static BitmapImage LoadBitmapImage(string path, int imgWidth, int imgHeight)
        {
            try
            {
                using MagickImage image = new MagickImage(path);

                if (Settings.DownsizeImageToggle)
                {
                    Rect r = WpfScreen.GetScreenFrom(Application.Current.MainWindow).ScreenBounds;
                    if (imgWidth > r.Width || imgHeight > r.Height)
                        image.Resize((int)(imgWidth * ScaleToBox(imgWidth, (int)r.Width, imgHeight, (int)r.Height)), 0);
                }
                image.AutoOrient();

                return ToBitmapImage(image.ToByteArray());
            }
            catch
            {
                return null;
            }
        }*/

        /*public static BitmapImage ByteArrayToBitmapImage(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }*/

        /*public static Rotation OrientationToRotation(OrientationType ot) => ot switch
        {
            OrientationType.TopLeft     => Rotation.Rotate0,
            OrientationType.TopRight    => Rotation.Rotate90,
            OrientationType.BottomRight => Rotation.Rotate180,
            OrientationType.BottomLeft  => Rotation.Rotate270,
            OrientationType.LeftTop     => Rotation.Rotate0,
            OrientationType.RightTop    => Rotation.Rotate90,
            OrientationType.RightBottom => Rotation.Rotate180,
            OrientationType.LeftBotom   => Rotation.Rotate270,
            _                           => Rotation.Rotate0,
        };*/

        /*private int ParseStringToOnlyInt(string input)
        {
            return int.Parse(string.Join("", input.Where(x => char.IsDigit(x))));
        }*/
    }

    /// <summary>
    /// Used to loosely label the type of file it is
    /// </summary>
    public enum FileMediaType
    {
        Image = 0,
        Gif = 1,
        Video = 2,
    }
}
