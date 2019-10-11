using System.Windows.Media.Imaging;

namespace FIVStandard.Views
{
    public partial class ThumbnailItemData
    {
        private string thumbnailName;

        public string ThumbnailName
        {
            get
            {
                return thumbnailName;
            }
            set
            {
                thumbnailName = value;
            }
        }

        private BitmapImage thumbnailImage;

        public BitmapImage ThumbnailImage
        {
            get
            {
                return thumbnailImage;
            }
            set
            {
                thumbnailImage = value;
            }
        }
    }
}