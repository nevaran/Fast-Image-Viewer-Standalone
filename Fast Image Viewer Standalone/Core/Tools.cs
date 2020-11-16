using FIVStandard.Views;
using ImageMagick;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static FIVStandard.Core.SettingsStore;

namespace FIVStandard.Core
{
    static class Tools
    {
        public static BitmapSource LoadImage(string path, int imgWidth, int imgHeight)
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

                BitmapSource bms = image.ToBitmapSource();
                bms.Freeze();

                return bms;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a thumbnail-sized image (if not already set), actual width and actual height of the image and assigns it to the given ThumbnailItemData.
        /// </summary>
        public static void GetImageInformation(string path, ThumbnailItemData ImageItem)
        {
            if (ImageItem.ThumbnailImage is null)
            {
                Task.Run(() => LoadSingleThumbnailData(ImageItem, path, false));
            }

            if (ImageItem.ImageWidth != 0)//if we already have a set width in the item data, use it instead
            {
                return;
            }

            MagickImageInfo image = new MagickImageInfo(path);
            ImageItem.ImageWidth = image.Width;
            ImageItem.ImageHeight = image.Height;
        }

        /// <summary>
        /// Load a single thumbnail image item data to the defined position (via ThumbnailItemData reference).
        /// </summary>
        /// <param name="name"> The Name + Extension of the file</param>
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
        /// <param name="itemData">Used for saving the image width, and height in the given ThumbnailItemData</param>
        /// <returns></returns>
        public static void LoadThumbnailData(string path, ThumbnailItemData tid)
        {
            if (!File.Exists(path)) return;

            try
            {
                var settings = new MagickReadSettings
                {
                    Width = Settings.ThumbnailRes
                };

                using MagickImage image = new MagickImage(path, settings);
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
        /// <returns></returns>
        public static bool IsOfType(string file, string[] extensions)
        {
            string ext = Path.GetExtension(file.ToLower());
            if (extensions.Any(ext.Contains)) return true;

            return false;
        }

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

        /*public static BitmapImage ToBitmapImage(byte[] array)
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

        /*public static Rotation GetOrientationRotation(OrientationType ot) => ot switch
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
