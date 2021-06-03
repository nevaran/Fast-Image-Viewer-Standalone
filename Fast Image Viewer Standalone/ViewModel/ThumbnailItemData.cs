using FIVStandard.Core;
using FIVStandard.Model;
using System.Windows.Media.Imaging;

namespace FIVStandard.ViewModel
{
    public sealed class ThumbnailItemData : PropertyChangedBase
	{
        public static SettingsJson Settings { get; set; }//Easier access for the thumbnail data, since we will always have just one instance of this

		private string _thumbnailName;
		public string ThumbnailName
		{
			get => _thumbnailName;
			set => SetField(ref _thumbnailName, value);
		}

		private BitmapSource _thumbnailImage;
		public BitmapSource ThumbnailImage
		{
			get => _thumbnailImage;
			set => SetField(ref _thumbnailImage, value);
		}

		private int _imageWidth = 0;
		public int ImageWidth
		{
			get => _imageWidth;
			set => SetField(ref _imageWidth, value);
		}

		private int _imageHeight = 0;
		public int ImageHeight
		{
			get => _imageHeight;
			set => SetField(ref _imageHeight, value);
		}

		private bool _isAnimated = false;
		public bool IsAnimated
		{
			get => _isAnimated;
			set => SetField(ref _isAnimated, value);
		}
    }
}