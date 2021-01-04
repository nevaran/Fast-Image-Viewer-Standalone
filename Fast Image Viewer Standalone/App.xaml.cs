using ImageMagick;
using System;
using System.Windows;
using Unosquare.FFME;

#pragma warning disable CA1416

namespace FIVStandard
{
    public partial class App : Application
    {
        public static string StartupPath { get; set; }//program startup path

        private App()
        {
            StartupPath = AppDomain.CurrentDomain.BaseDirectory;

            MagickAnyCPU.CacheDirectory = StartupPath;

            Library.FFmpegDirectory = @$"{StartupPath}\ffmpeg\bin";
            Library.FFmpegLoadModeFlags = FFmpeg.AutoGen.FFmpegLoadMode.MinimumFeatures;
            //Library.LoadFFmpeg();
        }
    }
}