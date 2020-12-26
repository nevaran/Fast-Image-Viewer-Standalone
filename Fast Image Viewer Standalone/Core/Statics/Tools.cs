using FIVStandard.Utils;
using FIVStandard.ViewModel;
using ImageMagick;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static FIVStandard.Core.SettingsStore;

namespace FIVStandard.Core
{
    static class Tools
    {
        public static Task<BitmapSource> LoadImage(string path, int imgWidth, int imgHeight, MainWindow mainWindow, CancellationToken ct)
        {
            if (!File.Exists(path))
                return Task.FromResult((BitmapSource)null);

            if (ct.IsCancellationRequested)
                return Task.FromResult((BitmapSource)null);

            using MagickImage image = new MagickImage(path);
            
            if (Settings.DownsizeImageToggle)
            {
                Nullable<Rect> nullableRect = null; // Is Rect a struct? Hope not.1

                Application.Current.Dispatcher.Invoke(() => { nullableRect = WpfScreen.GetScreenFrom(Application.Current.MainWindow).ScreenBounds;});

                if (!nullableRect.HasValue) {
                    throw new Exception("I'm a bad coder, and don't deserve to have Rectangles. CIRCLES FTW!");
                }

                var r = nullableRect.Value;
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

        /// <summary>
        /// Gets a thumbnail-sized image (if not already set), actual width and actual height of the image and assigns it to the given ThumbnailItemData.
        /// </summary>
        public static void GetImageInformation(string path, ThumbnailItemData ImageItem, MagickImageInfo magickInfo)
        {
            if (ImageItem.ThumbnailImage is null)
            {
                Task.Run(() => LoadSingleThumbnailData(ImageItem, path, false));
            }

            if (ImageItem.ImageWidth != 0)//if we already have a set width in the item data, use it instead
            {
                return;
            }

            magickInfo.Read(path);
            ImageItem.ImageWidth = magickInfo.Width;
            ImageItem.ImageHeight = magickInfo.Height;
        }

        /// <summary>
        /// Load a single thumbnail image item data to the defined position (via ThumbnailItemData reference).
        /// </summary>
        /// <param name="tid"> Information of the file, including a thumbnail</param>
        /// <param name="fullPath"> Complete path to the file, including the name and extension</param>
        /// <param name="overrideThumbnail"> If true: replace the thumbnail even if there is already one generated</param>
        /// <returns></returns>
        public static void LoadSingleThumbnailData(ThumbnailItemData tid, string fullPath, bool overrideThumbnail)
        {
            if (overrideThumbnail || tid.ThumbnailImage is null)//load thumbnail only if its set for override or is empty
                LoadThumbnailData(fullPath, tid);
        }

        /// <summary>
        /// Gets a resized version of the image file, and gets it's original size (wdith and height)
        /// </summary>
        /// <param name="path">The full path to the file, including extension</param>
        /// <param name="tid">Used for saving the image width, and height in the given ThumbnailItemData</param>
        /// <returns></returns>
        public static void LoadThumbnailData(string path, ThumbnailItemData tid)
        {
            if (Path.GetExtension(path) == ".webm" || !File.Exists(path)) return;

            try
            {
                /*var settings = new MagickReadSettings
                {
                    Width = Settings.ThumbnailRes
                };*/

                using MagickImage image = new MagickImage(path/*, settings*/);
                image.AutoOrient();

                image.Thumbnail(Settings.ThumbnailRes, 0);

                tid.ImageWidth = image.BaseWidth;
                tid.ImageHeight = image.BaseHeight;
                tid.ThumbnailImage = image.ToBitmapSource();
                tid.ThumbnailImage.Freeze();
            }
            catch
            {
                tid.ThumbnailImage = null;
            }
        }

        /// <summary>
        /// Checks if the string has an extension of the given valid types
        /// </summary>
        public static bool IsOfType(string file, string[] extensions)
        {
            string ext = Path.GetExtension(file.ToLower());
            if (extensions.Any(ext.Contains)) return true;

            return false;
        }

        /// <summary>
        /// Returns a scale multiplier that can be used for resizing width and height of an image to the given required size while keeping aspect ratios
        /// </summary>
        /// <param name="w">Image width</param>
        /// <param name="sw">Target width</param>
        /// <param name="h">Image height</param>
        /// <param name="sh">Target height</param>
        /// <returns></returns>
        private static double ScaleToBox(double w, double sw, double h, double sh)
        {
            double scaleWidth = sw / w;
            double scaleHeight = sh / h;

            double scale = Math.Min(scaleWidth, scaleHeight);

            return scale;
        }

        public static bool IsAnimatedExtension(string ext) => ext switch
        {
            ".gif" => true,
            ".webm" => true,
            _ => false,
        };

        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            
            var bitmapSource = BitmapSource.Create(
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
            Random rnd = new Random();
            string[] rnj = new string[] {
                        @"owo",
                        @"uwu",
                        @"ゴ ゴ ゴ ゴ",
                        @"I am Lagnar",
                        @"Made by ねヴぁらん",
                        @"¯\_(ツ)_/¯",
            };

            if (rnd.Next(0, 10) == 0)//10% chance to show meme
            {
                return rnj[rnd.Next(0, rnj.Length)];
            }

            return "";
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
}
