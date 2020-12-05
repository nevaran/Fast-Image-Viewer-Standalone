using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FIVStandard.Core
{
    public static class CopyFileToClipboard
    {
        const string CFSTR_PERFORMEDDROPEFFECT = "Preferred DropEffect";

        public static void ImageCopyToClipboard(BitmapSource img)
        {
            if (img is null) return;

            Clipboard.Clear();
            Clipboard.SetImage(img);
        }

        public static void FileCutToClipBoard(string path)
        {
            DataObject data = new DataObject();
            data.SetFileDropList(new StringCollection() { path });
            data.SetData(CFSTR_PERFORMEDDROPEFFECT, DragDropEffects.Move);

            Clipboard.Clear();
            Clipboard.SetDataObject(data, true);
        }

        /*public void GifCopyToClipboard(Uri img)
        {
            if (img is null) return;

            Clipboard.Clear();
            Clipboard.SetData(DataFormats.GetDataFormat("GIF").Name, img);
        }*/

        /*public static void FileCopyToClipboard(string path)
        {
            StringCollection paths = new StringCollection
            {
                path
            };
            
            Clipboard.Clear();
            Clipboard.SetFileDropList(paths);
        }*/

        /*public static void DataCopyToClipboard(string obj)
        {
            Clipboard.Clear();
            Clipboard.SetDataObject(obj, true);
        }*/
    }
}