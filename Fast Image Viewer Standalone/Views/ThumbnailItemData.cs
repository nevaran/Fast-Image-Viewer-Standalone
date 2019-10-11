using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace FIVStandard.Views
{
    public partial class ThumbnailItemData : INotifyPropertyChanged
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