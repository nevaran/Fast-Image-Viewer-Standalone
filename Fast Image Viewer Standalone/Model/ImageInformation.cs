using FIVStandard.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FIVStandard.Model
{
    public class ImageInformation : INotifyPropertyChanged
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
                OnPropertyChanged();
                OnPropertyChanged("ImgResStringFormat");
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
                OnPropertyChanged();
                OnPropertyChanged("ImgResStringFormat");
            }
        }

        public string ImgResStringFormat
        {
            get
            {
                if (ImgWidth == 0 || ImgHeight == 0)
                {
                    return Tools.RnJesus();
                }
                else
                    return $"{ImgWidth.ToString()}x{ImgHeight.ToString()}";
            }
        }

        private bool fromUrl = false;

        /// <summary>
        /// is the image loaded normally or from a url
        /// </summary>
        public bool FromUrl
        {
            get
            {
                return fromUrl;
            }
            set
            {
                fromUrl = value;
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
