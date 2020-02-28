using FIVStandard.Views;
using ImageMagick;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static FIVStandard.Core.SettingsStore;

namespace FIVStandard.Core
{
    static class Tools
    {
        public static BitmapImage LoadImage(string path, int imgWidth, int imgHeight, Rotation imgRotation)
        {
            BitmapImage imgTemp = new BitmapImage();
            imgTemp.BeginInit();
            imgTemp.CacheOption = BitmapCacheOption.OnLoad;//TODO: remove this so it loads faster - needs to make workaround for deleting and cutting file from file lockup
            //imgTemp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;//TODO: remove this so it loads faster - needs to make workaround for deleting file
            
            /*string ext = Path.GetExtension(path);

            if(ext == ".webp")
            {
                try
                {
                    var fbytes = File.ReadAllBytes(path);
                    var decoder = new Imazen.WebP.SimpleDecoder();
                    var bitmap = decoder.DecodeFromBytes(fbytes, fbytes.Length);

                    using MemoryStream stream = new MemoryStream();
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    imgTemp.StreamSource = stream;

                    if (Settings.DownsizeImageToggle)
                    {
                        Rect r = WpfScreen.GetScreenFrom(Application.Current.MainWindow).ScreenBounds;

                        if (imgWidth > r.Width || imgHeight > r.Height)
                            imgTemp.DecodePixelWidth = (int)(imgWidth * ScaleToBox(imgWidth, (int)r.Width, imgHeight, (int)r.Height));
                    }
                    if (imgRotation != Rotation.Rotate0)
                        imgTemp.Rotation = imgRotation;

                    imgTemp.EndInit();
                    imgTemp.Freeze();
                }
                catch
                {

                }
            }*/

            using FileStream stream = File.OpenRead(path);
            imgTemp.StreamSource = stream;

            if (Settings.DownsizeImageToggle)
            {
                Rect r = WpfScreen.GetScreenFrom(Application.Current.MainWindow).ScreenBounds;

                if (imgWidth > r.Width || imgHeight > r.Height)
                    imgTemp.DecodePixelWidth = (int)(imgWidth * ScaleToBox(imgWidth, (int)r.Width, imgHeight, (int)r.Height));
            }
            if (imgRotation != Rotation.Rotate0)
                imgTemp.Rotation = imgRotation;

            imgTemp.EndInit();
            imgTemp.Freeze();

            //MessageBox.Show(imgTemp.HasAnimatedProperties.ToString());
            return imgTemp;
        }

        /// <summary>
        /// Gets the width, height, and orientation of the image and assigns it to the given ThumbnailItemData.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ImageItem"></param>
        /// <returns></returns>
        public static (int imgWidth, int imgHeight, Rotation imgRotation) GetImageInformation(string path, ThumbnailItemData ImageItem)
        {
            var info = (imgWidth:0, imgHeight:0, imgRotation:Rotation.Rotate0);

            if (ImageItem.ThumbnailImage is null)
            {
                Task.Run(() => LoadSingleThumbnail(ImageItem, path, false));
            }

            if (ImageItem.ImageOrientation != null)//if we have a set orientation in the item data, use it instead
            {
                info.imgWidth = ImageItem.ImageWidth;
                info.imgHeight = ImageItem.ImageHeight;
                info.imgRotation = (Rotation)ImageItem.ImageOrientation;
            }

            using (MagickImage image = new MagickImage(path))
            {
                Rotation imgRotation = GetOrientationRotation(image.Orientation);//get rotation

                info.imgWidth = image.BaseWidth;
                info.imgHeight = image.BaseHeight;
                info.imgRotation = imgRotation;
            }

            return info;
        }

        /// <summary>
        /// Load a single image to the defined position (via file name).
        /// </summary>
        /// <param name="name"> The Name + Extension of the file</param>
        /// <param name="fullPath"> Complete path to the file, including the name and extension</param>
        /// <param name="overrideThumbnail"> If true: replace the thumbnail even if there is already one generated</param>
        /// <returns></returns>
        public static void LoadSingleThumbnail(ThumbnailItemData tid, string fullPath, bool overrideThumbnail)
        {
            if (overrideThumbnail || tid.ThumbnailImage is null)
                tid.ThumbnailImage = GetThumbnail(fullPath, tid);

            /*if (tid.IsAnimated)//TODO add option "Animate thumbnail gifs" check
            {
                //tid.ThumbnailMedia = new Uri(Path.Combine(ActiveFolder, tid.ThumbnailName));
            }
            else
            {
                //TODO put normal thumbnail load here
            }*/
        }

        /// <summary>
        /// Gets a resized version of the image file, and gets it's orientation and size (wdith and height)
        /// </summary>
        /// <param name="path">The full path to the file, including extension</param>
        /// <param name="itemData">Used for saving the image orientation, width, and height in the given ThumbnailItemData</param>
        /// <returns></returns>
        public static BitmapImage GetThumbnail(string path, ThumbnailItemData itemData)
        {
            if (!File.Exists(path)) return null;

            BitmapImage imgTemp = new BitmapImage();
            using (FileStream stream = File.OpenRead(path))
            {
                imgTemp.BeginInit();
                imgTemp.CacheOption = BitmapCacheOption.OnLoad;
                imgTemp.StreamSource = stream;

                imgTemp.DecodePixelWidth = Settings.ThumbnailRes;
                //imgTemp.DecodePixelHeight = 80;

                using (MagickImage image = new MagickImage(path))
                {
                    Rotation imgRotation = GetOrientationRotation(image.Orientation);//get rotation

                    itemData.ImageWidth = image.BaseWidth;
                    itemData.ImageHeight = image.BaseHeight;
                    itemData.ImageOrientation = imgRotation;

                    if (imgRotation != Rotation.Rotate0)
                        imgTemp.Rotation = imgRotation;
                }

                imgTemp.EndInit();
                imgTemp.Freeze();
            }

            return imgTemp;
        }

        private static double ScaleToBox(double w, double sw, double h, double sh)
        {
            double scaleWidth = sw / w;
            double scaleHeight = sh / h;

            double scale = Math.Min(scaleWidth, scaleHeight);

            return scale;
        }

        /*private static string FileDialogAddType(string currentFilter, string addedType)TODO: finish options for choosing what file is automatically opened
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("*.JPG, *.JPEG, *.PNG, *.GIF, *.BMP, *ICO, *WEBP)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP;*.ICO;*.WEBP");//REFERENCE

            sb.Append("Image");
            //add info here
            sb.Append("|");
            //add file types here
            

            return sb.ToString();
        }*/

        public static bool IsAnimatedExtension(string ext) => ext switch
        {
            ".gif" => true,
            //".webp" => true,
            _ => false,
        };

        public static Rotation GetOrientationRotation(OrientationType ot) => ot switch
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
        };
    }
}
