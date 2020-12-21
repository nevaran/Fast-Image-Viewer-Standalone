using FIVStandard.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace FIVStandard.ViewModel
{
    public partial class ThumbnailItemData : INotifyPropertyChanged
    {
        public static SettingsJson Settings { get; set; }

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

        private BitmapSource thumbnailImage;

        public BitmapSource ThumbnailImage
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}