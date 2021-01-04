using ImageMagick;
using System;
using System.Windows;
using Unosquare.FFME;

#pragma warning disable CA1416

namespace FIVStandard
{
    public partial class App : Application
    {
        private App()
        {
            MagickAnyCPU.CacheDirectory = AppDomain.CurrentDomain.BaseDirectory;

            Library.FFmpegDirectory = @$"{AppDomain.CurrentDomain.BaseDirectory}\ffmpeg\bin";
            Library.FFmpegLoadModeFlags = FFmpeg.AutoGen.FFmpegLoadMode.MinimumFeatures;
            //Library.LoadFFmpeg();
        }
    }
}