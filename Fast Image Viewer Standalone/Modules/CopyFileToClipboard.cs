using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FIVStandard.Modules
{
    public class CopyFileToClipboard
    {
        const string CFSTR_PERFORMEDDROPEFFECT = "Preferred DropEffect";

        public void ImageCopyToClipboard(BitmapSource img)
        {
            if (img == null) return;

            Clipboard.Clear();
            Clipboard.SetImage(img);
        }

        public void FileCopyToClipboard(string path)
        {
            StringCollection paths = new StringCollection
            {
                path
            };
            
            Clipboard.Clear();
            Clipboard.SetFileDropList(paths);
        }

        public void FileCutToClipBoard(string path)
        {
            /*DataObject data = new DataObject();

            StringCollection paths = new StringCollection
            {
                path
            };

            data.SetFileDropList(paths);
            data.SetData("Preferred Dropeffect", DragDropEffects.Move);

            Clipboard.Clear();
            Clipboard.SetDataObject(data, true);*/

            DataObject data = new DataObject();
            data.SetFileDropList(new StringCollection() { path });
            data.SetData(CFSTR_PERFORMEDDROPEFFECT, DragDropEffects.Move);

            Clipboard.Clear();
            Clipboard.SetDataObject(data, true);
        }

        public void DataCopyToClipboard(string obj)
        {
            Clipboard.Clear();
            Clipboard.SetDataObject(obj, true);
        }
    }
}