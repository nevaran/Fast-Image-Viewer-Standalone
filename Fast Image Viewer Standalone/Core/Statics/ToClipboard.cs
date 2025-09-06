using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FIVStandard.Core
{
    static class ToClipboard
    {

        public static void ImageCopyToClipboard(BitmapSource img)
        {
            if (img is null) return;

            Clipboard.Clear();
            Clipboard.SetImage(img);
        }

        public static void FileCutToClipBoard(string path)
        {
            // A list of files to be cut.
            StringCollection fileCollection = [path];

            // Create a DataObject to hold the file list and the desired effect.
            DataObject data = new DataObject();

            // Add the list of files to be cut.
            data.SetFileDropList(fileCollection);

            // Specify the PREFERRED action is a MOVE (cut).
            // This must be a MemoryStream containing the integer value of the DragDropEffects enum.
            byte[] moveEffect = [(byte)DragDropEffects.Move, 0, 0, 0];
            MemoryStream dropEffectStream = new MemoryStream(moveEffect);
            data.SetData("Preferred DropEffect", dropEffectStream);

            // Set the data to the clipboard.
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