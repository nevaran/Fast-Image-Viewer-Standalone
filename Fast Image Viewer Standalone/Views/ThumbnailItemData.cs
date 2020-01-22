using FIVStandard.Modules;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace FIVStandard.Views
{
    public partial class ThumbnailItemData : INotifyPropertyChanged
    {
        public static SettingsManager Settings { get; set; }

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
                OnPropertyChanged();
            }
        }

        /*private Uri thumbnailMedia = null;

        public Uri ThumbnailMedia
        {
            get
            {
                return thumbnailMedia;
            }
            set
            {
                thumbnailMedia = value;
                OnPropertyChanged();
            }
        }*/

        private int imageWidth = 0;

        public int ImageWidth
        {
            get
            {
                return imageWidth;
            }
            set
            {
                imageWidth = value;
                OnPropertyChanged();
            }
        }

        private int imageHeight = 0;

        public int ImageHeight
        {
            get
            {
                return imageHeight;
            }
            set
            {
                imageHeight = value;
                OnPropertyChanged();
            }
        }

        private bool isAnimated = false;

        public bool IsAnimated
        {
            get
            {
                return isAnimated;
            }
            set
            {
                isAnimated = value;
                OnPropertyChanged();
            }
        }

        private Rotation? imageOrientation = null;//if null, it will be counted as if the orientation is not yet set up

        public Rotation? ImageOrientation
        {
            get
            {
                return imageOrientation;
            }
            set
            {
                imageOrientation = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}