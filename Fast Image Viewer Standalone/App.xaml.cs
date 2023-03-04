﻿using System;
using System.Windows;
using Unosquare.FFME;

namespace FIVStandard
{
    public partial class App : Application
    {
        private App()
        {
            Library.FFmpegDirectory = @$"{AppDomain.CurrentDomain.BaseDirectory}\ffmpeg\bin";
            Library.FFmpegLoadModeFlags = FFmpeg.AutoGen.FFmpegLoadMode.MinimumFeatures;
            //Library.LoadFFmpeg();
        }
    }
}