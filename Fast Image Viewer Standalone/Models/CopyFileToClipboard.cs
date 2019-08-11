using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FIVStandard.Models
{
    public class CopyFileToClipboard
    {
        public void ImageToClipboard(BitmapSource img)
        {
            if (img == null) return;

            Clipboard.SetImage(img);
        }

        public void FileToClipboard(string path)
        {
            StringCollection paths = new StringCollection
            {
                path
            };
            
            Clipboard.SetFileDropList(paths);
        }

        public void DataToClipboard(string obj)
        {
            Clipboard.SetDataObject(obj, true);
        }
    }
}