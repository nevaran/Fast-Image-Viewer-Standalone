using FIVStandard.Core;
using FIVStandard.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace FIVStandard.ViewModel
{
    public sealed class ThumbnailItemData : INotifyPropertyChanged
    {
        public static SettingsJson Settings { get; set; }//Easier access for the thumbnail data, since we will always have just one instance of this

        private string _thumbnailName;
        public string ThumbnailName
        {
            get => _thumbnailName;
            set
            {
                _thumbnailName = value;
                OnPropertyChanged();
            }
        }

        private BitmapSource _thumbnailImage;
        public BitmapSource ThumbnailImage
        {
            get => _thumbnailImage;
            set
            {
                _thumbnailImage = value;
                OnPropertyChanged();
            }
        }

        private int _imageWidth;
        public int ImageWidth
        {
            get => _imageWidth;
            set
            {
                _imageWidth = value;
                OnPropertyChanged();
            }
        }

        private int _imageHeight;
        public int ImageHeight
        {
            get => _imageHeight;
            set
            {
                _imageHeight = value;
                OnPropertyChanged();
            }
        }

        private FileMediaType _fileType;
        public FileMediaType FileType
        {
            get => _fileType;
            set
            {
                _fileType = value;
                OnPropertyChanged();
                OnPropertyChanged("IsAnimated");
            }
        }

        public bool IsAnimated => FileType != FileMediaType.Image;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}