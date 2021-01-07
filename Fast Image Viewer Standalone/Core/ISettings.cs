
using System.Collections.Generic;

namespace FIVStandard.Core
{
    interface ISettings
    {
        bool DownsizeImageToggle { get; }

        int ThumbnailRes { get; }

        string[] ThemeAccents { get; }

        int ThemeAccentDropIndex { get; }
    }
}
