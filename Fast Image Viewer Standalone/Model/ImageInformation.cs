using FIVStandard.Core.Statics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FIVStandard.Model
{
    public sealed class ImageInformation : INotifyPropertyChanged
    {
        private int imgWidth = 0;

        public int ImgWidth
        {
            get
            {
                return imgWidth;
            }
            set
            {
                imgWidth = value;
                //OnPropertyChanged();
                //OnPropertyChanged("ImgInfoStringFormat");
            }
        }

        private int imgHeight = 0;

        public int ImgHeight
        {
            get
            {
                return imgHeight;
            }
            set
            {
                imgHeight = value;
                //OnPropertyChanged();
                //OnPropertyChanged("ImgInfoStringFormat");
            }
        }

        private string fileSize = "";

        public string FileSize
        {
            get
            {
                return fileSize;
            }
            set
            {
                fileSize = value;
                OnPropertyChanged();
                OnPropertyChanged("ImgWidth");
                OnPropertyChanged("ImgHeight");
                OnPropertyChanged("ImgInfoStringFormat");
            }
        }

        public string ImgInfoStringFormat
        {
            get
            {
                if (ImgWidth == 0 || ImgHeight == 0)
                {
                    return Tools.RnJesus();
                }
                else
                    return $"{FileSize} • {ImgWidth}x{ImgHeight}";
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
